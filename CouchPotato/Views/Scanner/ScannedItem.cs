using CommunityToolkit.Mvvm.ComponentModel;

using CouchPotato.DbModel;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Windows.Devices.Geolocation;

namespace CouchPotato.Views.Scanner;

public enum Action
{
    Skip, Add, Update
}

public partial class ScannedItem : ObservableObject
{
    [ObservableProperty]
    private string _filename = string.Empty;
    private string _searchName = string.Empty;
    [ObservableProperty]
    private string _searchTitle = string.Empty;
    [ObservableProperty]
    private ObservableCollection<Video> _possibleVideos = [];
    private Video? _video = null;
    [ObservableProperty]
    private Action _action = Action.Skip;

    public ScannedItem(string filename, List<Tuple<string[], Video>> allVideos)
    {
        _filename = filename;
        _searchName = System.IO.Path.GetFileNameWithoutExtension(filename);
        var tokens = Tokenize(filename);
        var candidates = allVideos
            .Select(v => new
            {
                Video = v.Item2,
                Tokens = v.Item1,
                Distance = AsymmetricLevenshtein(tokens, v.Item1)
            })
            .OrderBy(t => t.Distance)
            .Take(5)
            .ToList();
        foreach (var candidate in candidates)
        {
            PossibleVideos.Add(candidate.Video);
        }
        if (candidates.Count > 0)
        {
            var best = candidates.First();
            if (best.Distance < 1.0)
            {
                Action = Action.Update;
                Video = best.Video;
                return;
            }
        }
        Video = null;
        Action = Action.Add;
    }

    public double Confidence
    {
        get {
            if (Video is null) return 0.0;
            var fileTokens = Tokenize(SearchName);
            var titleTokens = Tokenize(Video.Title);
            var distance = AsymmetricLevenshtein(fileTokens, titleTokens);
            return 1.0 - (distance / titleTokens.Length);
        }
    }

    public Video? Video
    {
        get => _video;
        set {
            SetProperty(ref _video, value);
            OnPropertyChanged(nameof(Confidence));
            if (value is not null)
            {
                IsUpdated = true;
            }
        }
    }

    public string SearchName
    {
        get => _searchName;
        set {
            SetProperty(ref _searchName, value);
            using var db = new DataContext();
            PossibleVideos.Clear();
            foreach (var video in db.Videos.Where(v => v.Title.ToLower().Contains(_searchName.ToLower())))
            {
                PossibleVideos.Add(video);
            }
        }
    }

    public bool IsSkipped
    {
        get => Action == Action.Skip;
        set {
            if (value)
            {
                Action = Action.Skip;
                Video = null;
                OnPropertyChanged(nameof(IsSkipped));
            }
        }
    }

    public bool IsAdded
    {
        get => Action == Action.Add;
        set {
            if (value)
            {
                Action = Action.Add;
                Video = null;
                OnPropertyChanged(nameof(IsAdded));
            }
        }
    }

    public bool IsUpdated
    {
        get => Action == Action.Update;
        set {
            if (value)
            {
                Action = Action.Update;
                OnPropertyChanged(nameof(IsUpdated));
            }
        }
    }

    private static string RemoveDiacritics(string input)
    {
        var normalizedString = input.Normalize(NormalizationForm.FormD);
        var stringBuilder = new System.Text.StringBuilder();
        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string[] Tokenize(string input)
    {
        // Convert to lowercase, no accents
        input = RemoveDiacritics(input).ToLowerInvariant();
        input = input.Replace("œ", "oe");
        input = input.Replace("æ", "ae");

        var stopWords = new HashSet<string>
        {
            "1080p", "720p", "4k", "bluray", "brrip", "dvdrip", "hdrip", "x264", "x265", "webrip", "vostfr"
        };

        // Replace non-alphanumeric characters with spaces
        input = Regex.Replace(input, @"[^a-z0-9]+", " ");

        // Split by whitespace and remove empty entries
        var tokens = input.Split([' '], StringSplitOptions.RemoveEmptyEntries);

        return tokens.Where(token => !stopWords.Contains(token)).ToArray();
    }

    private static double Levenshtein(string[] fileTokens, string[] titleTokens)
    {
        int n = fileTokens.Length;
        int m = titleTokens.Length;

        // Matrice de coûts
        double[,] d = new double[n + 1, m + 1];

        // Initialisation
        for (int i = 0; i <= n; i++) d[i, 0] = i;
        for (int j = 0; j <= m; j++) d[0, j] = j;

        // Remplissage de la matrice
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                bool isMatch = fileTokens[i - 1] == titleTokens[j - 1];
                double matchCost = isMatch ? 0.0 : 1.0;
                double deleteFileWord = d[i - 1, j] + 1; // Le fichier a un mot en trop
                double insertTitleWord = d[i, j - 1] + 1; // Le fichier a raté un mot du titre
                double substitution = d[i - 1, j - 1] + matchCost; // Comparaison directe
                d[i, j] = Math.Min(Math.Min(deleteFileWord, insertTitleWord), substitution);
            }
        }
        return d[n, m];
    }

    private static double AsymmetricLevenshtein(string[] fileTokens, string[] titleTokens)
    {
        int n = fileTokens.Length;
        int m = titleTokens.Length;

        // Matrice de coûts
        double[,] d = new double[n + 1, m + 1];

        // --- PONDÉRATIONS (C'est ici qu'on "trafique" l'algo) ---
        const double CostSkipFileWord = 0.0;  // Pénalité très faible pour le bruit (1080p, etc.)
        const double CostSkipTitleWord = 1.0; // Pénalité énorme : le fichier DOIT contenir les mots du titre
        const double CostMismatch = 1.0;      // Pénalité moyenne pour une typo (Subst)

        // Initialisation
        d[0, 0] = 0;

        // Si le titre est vide mais le fichier a des mots -> Coût faible (bruit)
        for (int i = 1; i <= n; i++) d[i, 0] = d[i - 1, 0] + CostSkipFileWord;

        // Si le fichier est vide mais le titre a des mots -> Coût prohibitif (mot manquant)
        for (int j = 1; j <= m; j++) d[0, j] = d[0, j - 1] + CostSkipTitleWord;

        // Remplissage de la matrice
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                // Vérification égalité (ici égalité stricte, on pourrait mettre un petit Fuzzy sur les caractères)
                bool isMatch = fileTokens[i - 1] == titleTokens[j - 1];
                double matchCost = isMatch ? 0.0 : CostMismatch;

                double deleteFileWord = d[i - 1, j] + CostSkipFileWord; // Le fichier a un mot en trop
                double insertTitleWord = d[i, j - 1] + CostSkipTitleWord; // Le fichier a raté un mot du titre
                double substitution = d[i - 1, j - 1] + matchCost;      // Comparaison directe

                d[i, j] = Math.Min(Math.Min(deleteFileWord, insertTitleWord), substitution);
            }
        }

        return d[n, m];
    }
}
