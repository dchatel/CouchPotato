using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using CouchPotato.DbModel;
using CouchPotato.Views.VideoEditor;

using Microsoft.EntityFrameworkCore;

namespace CouchPotato.Views.VideoExplorer;

public partial class VideoExplorerViewModel : ContentViewModel
{
    [ObservableProperty]
    private bool _isAdvancedSearchDialogOpened;
    [ObservableProperty]
    private bool _isSearching = false;
    [ObservableProperty]
    private ObservableCollection<VideoSearchResultViewModel>? _searchResults;
    [ObservableProperty]
    private string _title = "";
    [ObservableProperty]
    private string _actors = "";
    [ObservableProperty]
    private string _roles = "";

    public bool DateActive => HasDateEquals || HasDateBefore || HasDateAfter;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DateActive))]
    private bool _hasDateBefore = false;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DateActive))]
    private bool _hasDateEquals = false;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DateActive))]
    private bool _hasDateAfter = false;
    [ObservableProperty]
    private int? _year = null;

    [ObservableProperty]
    private string _digitalStorageCode = "";

    public bool DigitalResolutionActive => HasDigitalResolutionLesser || HasDigitalResolutionEquals || HasDigitalResolutionGreater;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DigitalResolutionActive))]
    private bool _hasDigitalResolutionLesser = false;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DigitalResolutionActive))]
    private bool _hasDigitalResolutionEquals = false;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DigitalResolutionActive))]
    private bool _hasDigitalResolutionGreater = false;
    [ObservableProperty]
    private string _digitalResolution = "";

    public bool TmdbRatingActive => HasTmdbRatingEquals || HasTmdbRatingLesser || HasTmdbRatingGreater;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TmdbRatingActive))]
    private bool _hasTmdbRatingLesser = false;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TmdbRatingActive))]
    private bool _hasTmdbRatingEquals = false;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TmdbRatingActive))]
    private bool _hasTmdbRatingGreater = false;
    [ObservableProperty]
    private double? _tmdbRating = null;

    public bool PersonalRatingActive => HasPersonalRatingEquals || HasPersonalRatingLesser || HasPersonalRatingGreater;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PersonalRatingActive))]
    private bool _hasPersonalRatingLesser = false;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PersonalRatingActive))]
    private bool _hasPersonalRatingEquals = false;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PersonalRatingActive))]
    private bool _hasPersonalRatingGreater = false;
    [ObservableProperty]
    private int? _personalRating = null;

    [ObservableProperty]
    private int? _runtime = null;

    private VideoSearchResultViewModel? _selectedResult;
    private string _searchText = "";

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand AdvancedSearchCommand { get; }
    public ICommand ResetAdvancedSearchCommand { get; }

    [RelayCommand]
    private void OpenScanner()
    {
        var scanner = new Scanner.ScannerViewModel();
        _ = scanner.Show();
    }

    public VideoSearchResultViewModel? SelectedResult
    {
        get {
            _ = _selectedResult?.VideoViewer.LoadData();
            return _selectedResult;
        }
        set => SetProperty(ref _selectedResult, value);
    }

    public string SearchText
    {
        get => _searchText;
        set {
            _searchText = value;
            Task.Run(SearchAsync);
        }
    }

    public IEnumerable<Togglable<Genre>> Genres { get; }

    public VideoExplorerViewModel() : base(autoClose: false)
    {
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DateActive) && !DateActive) Year = null;
            if (e.PropertyName == nameof(DigitalResolutionActive) && !DigitalResolutionActive) DigitalResolution = "";
            if (e.PropertyName == nameof(TmdbRatingActive) && !TmdbRatingActive) TmdbRating = null;
            if (e.PropertyName == nameof(PersonalRatingActive) && !PersonalRatingActive) PersonalRating = null;
        };
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        AddCommand = new AsyncRelayCommand(Add);
        EditCommand = new AsyncRelayCommand(Edit);
        AdvancedSearchCommand = new AsyncRelayCommand(AdvancedSearch);
        ResetAdvancedSearchCommand = new RelayCommand(ResetAdvancedSearch);
        using var db = new DataContext();
        Genres = db.Genres.ToArray().Select(genre => new Togglable<Genre>(genre, isSelected: false)).ToArray();
    }

    private void ResetAdvancedSearch()
    {
        Title = "";
        Actors = "";
        Roles = "";
        foreach (var item in Genres)
        {
            item.IsSelected = false;
        }
        Year = null;
        HasDateBefore = false;
        HasDateEquals = false;
        HasDateAfter = false;
        DigitalStorageCode = "";
        DigitalResolution = "";
        HasDigitalResolutionLesser = false;
        HasDigitalResolutionEquals = false;
        HasDigitalResolutionGreater = false;
        TmdbRating = null;
        HasTmdbRatingLesser = false;
        HasTmdbRatingEquals = false;
        HasTmdbRatingGreater = false;
        PersonalRating = null;
        HasPersonalRatingLesser = false;
        HasPersonalRatingEquals = false;
        HasPersonalRatingGreater = false;
    }

    private async Task Add()
    {
        using var db = new DataContext();
        var video = new Video();
        db.Add(video);
        var editor = new VideoEditorViewModel(db, video, editionMode: false);
        if (await editor.Show())
        {
            await db.SaveChangesAsync();
            ImageChange.Apply();
            SelectedResult = new VideoSearchResultViewModel(VideoViewerViewModel.Create(video));
            SearchResults?.Add(SelectedResult);
        }
        else
        {
            ImageChange.Cancel();
        }
    }

    private async Task Edit()
    {
        if (SelectedResult is null) return;

        using var db = new DataContext();
        var video = SelectedResult.VideoViewer.Video.CopyFromDb(db);
        var editor = new VideoEditorViewModel(db, video, editionMode: true);
        if (await editor.Show())
        {
            if (editor.VideoWasRemoved)
            {
                db.Remove(video);
                await db.SaveChangesAsync();
                SearchResults?.Remove(SelectedResult);
                SelectedResult = null;
            }
            else
            {
                await db.SaveChangesAsync();
                ImageChange.Apply();

                var videoViewerViewModel = VideoViewerViewModel.Create(video);
                await videoViewerViewModel.LoadData();
                SelectedResult.VideoViewer = videoViewerViewModel;
            }
        }
        else
        {
            ImageChange.Cancel();
        }
    }

    private async Task SearchAsync()
    {
        IsSearching = true;

        using var db = new DataContext();
        SearchResults = [.. await db.Videos
            .Where(video => EF.Functions.Collate(video.Title, "NO_ACCENTS") == SearchText)
            .Select(video => new VideoSearchResultViewModel(VideoViewerViewModel.Create(video)))
            .ToArrayAsync()];
        IsSearching = false;
    }

    private async Task AdvancedSearch()
    {
        IsSearching = true;

        using var db = new DataContext();
        var selectedGenres = Genres.Where(g => g.IsSelected == true).Select(g => g.Value.Id).ToArray();
        var unselectedGenres = Genres.Where(g => g.IsSelected is null).Select(g => g.Value.Id).ToArray();

        var query = db.Videos.AsQueryable();
        if (!string.IsNullOrWhiteSpace(Title)) query = query.Where(video => EF.Functions.Collate(video.Title, "NO_ACCENTS") == Title);
        if (!string.IsNullOrWhiteSpace(Actors)) query = query.Where(video => Actors.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).All(actor => video.Roles.Any(role => EF.Functions.Collate(role.Person.Name, "NO_ACCENTS") == actor)));
        if (!string.IsNullOrWhiteSpace(Roles)) query = query.Where(video => Roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).All(role => video.Roles.Any(r => !string.IsNullOrEmpty(r.Characters) && EF.Functions.Collate(r.Characters, "NO_ACCENTS") == role)));

        if (selectedGenres.Length != 0) query = query.Where(video => selectedGenres.All(g => video.Genres.Any(vg => vg.Id == g)));
        if (unselectedGenres.Length != 0) query = query.Where(video => unselectedGenres.All(g => !video.Genres.Any(vg => vg.Id == g)));

        if (DateActive && Year is not null)
        {
            var pred = PredicateBuilder.False<Video>();
            if (HasDateBefore) pred = pred.Or(video => video.ReleaseDate < new DateTime((int)Year, 1, 1, 0, 0, 0));
            if (HasDateEquals) pred = pred.Or(video => video.ReleaseDate >= new DateTime((int)Year, 1, 1, 0, 0, 0) && video.ReleaseDate <= new DateTime((int)Year, 12, 31, 23, 59, 59));
            if (HasDateAfter) pred = pred.Or(video => video.ReleaseDate > new DateTime((int)Year, 12, 31, 23, 59, 59));
            query = query.Where(pred);
        }

        if (!string.IsNullOrWhiteSpace(DigitalStorageCode)) query = query.Where(video => EF.Functions.Collate(video.DigitalStorageCode, "NO_ACCENTS") == DigitalStorageCode);

        if (DigitalResolutionActive)
        {
            var pred = PredicateBuilder.False<Video>();
            if (HasDigitalResolutionLesser) pred = pred.Or(video => EF.Functions.Collate(video.DigitalResolution, "RESOLUTION_LESSER") == DigitalResolution);
            if (HasDigitalResolutionEquals) pred = pred.Or(video => EF.Functions.Collate(video.DigitalResolution, "RESOLUTION_EQUAL") == DigitalResolution);
            if (HasDigitalResolutionGreater) pred = pred.Or(video => EF.Functions.Collate(video.DigitalResolution, "RESOLUTION_GREATER") == DigitalResolution);
            query = query.Where(pred);
        }

        if (TmdbRatingActive && TmdbRating is not null)
        {
            var pred = PredicateBuilder.False<Video>();
            if (HasTmdbRatingLesser) pred = pred.Or(video => video.TmdbRating < TmdbRating * 10);
            if (HasTmdbRatingEquals) pred = pred.Or(video => video.TmdbRating == TmdbRating * 10);
            if (HasTmdbRatingGreater) pred = pred.Or(video => video.TmdbRating > TmdbRating * 10);
            query = query.Where(pred);
        }

        if (PersonalRatingActive && PersonalRating is not null)
        {
            var pred = PredicateBuilder.False<Video>();
            if (HasPersonalRatingLesser) pred = pred.Or(video => video.PersonalRating < PersonalRating);
            if (HasPersonalRatingEquals) pred = pred.Or(video => video.PersonalRating == PersonalRating);
            if (HasPersonalRatingGreater) pred = pred.Or(video => video.PersonalRating > PersonalRating);
            query = query.Where(pred);
        }

        var results = await query
            .Select(video => new VideoSearchResultViewModel(VideoViewerViewModel.Create(video)))
            .ToArrayAsync();

        SearchResults = [.. results];

        IsSearching = false;
    }
}

public partial class VideoSearchResultViewModel : ObservableObject
{
    [ObservableProperty]
    private VideoViewerViewModel _videoViewer;

    public VideoSearchResultViewModel(VideoViewerViewModel videoViewerViewModel)
    {
        VideoViewer = videoViewerViewModel;
    }
}