using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

using CouchPotato.DbModel;
using CouchPotato.Properties;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

using Loc = CouchPotato.Properties.Loc;

namespace CouchPotato.Views.MigratorDialog;

public class MigratorViewModel : ContentViewModel
{
    public string? TaskName { get; private set; }
    public bool TaskIsIndeterminate { get; private set; }
    public int TaskMaximum { get; private set; }
    public int TaskProgression { get; private set; }

    private readonly DataContext _db;
    private readonly DbModel.OtherDbModels.Videlib.DataContext _videlib;
    private readonly Dictionary<int, Genre> _dgenres = [];
    private readonly Dictionary<int, Person> _dpersons = [];
    private readonly Dictionary<int, Video> _dvideos = [];
    private readonly Dictionary<int, Season> _dseasons = [];

    public MigratorViewModel() : base(autoClose: false)
    {
        _db = new();
        _videlib = new();
    }

    //public override bool CanAutoClose => false;

    private bool AreMigrationsPending()
    {
        return _db.Database.GetPendingMigrations().Any();
    }

    protected override async void OnLoaded()
    {
#if DEBUG
        var result = MessageBox.Show("Reset Data?", "Reset", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
        if (result == MessageBoxResult.Yes)
        {
            _db.Database.EnsureDeleted();
            if (Directory.Exists("Images"))
                Directory.Delete("Images", recursive: true);
        }
#endif
        await Task.Delay(500);

        if (Config.Default.EnableAutoUpdates)
        {
            await UpdateApp();
        }

        if (!AreMigrationsPending())
        {
            Close(true);
            return;
        }

        try
        {
            if (!_db.Database.GetAppliedMigrations().Any())
            {
                try
                {
                    await Migrate_v0();
                }
                catch
                {
                    _db.Database.EnsureDeleted();
                    throw;
                }
            }
            TaskName = Loc.DatabaseUpdating;
            TaskIsIndeterminate = true;
            await _db.Database.MigrateAsync();
            Close(true);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
            TaskName = Loc.DatabaseMigrationFailed;
            await Task.Delay(5000);
            App.Current.MainWindow.Close();
            Close(false);
        }
    }

    private async Task UpdateApp()
    {
        TaskName = Loc.CheckForUpdates;
        TaskIsIndeterminate = true;
        var root = AppDomain.CurrentDomain.BaseDirectory;
        // Remove old files
        foreach (var oldFile in Directory.GetFiles(root, "*.old"))
            File.Delete(oldFile);

        using var client = new HttpClient();

        // Compare versions
        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        if (currentVersion is null)
            return;

        var r = await client.GetAsync("https://github.com/dchatel/CouchPotato/releases/latest");
        var lastVersion = r.RequestMessage?.RequestUri?.Segments.Last();
        if (lastVersion is null)
            return;

        if (currentVersion.CompareTo(lastVersion) < 0)
        {
            TaskName = Loc.UpdateFound;
            // New version available
            using var s = client.GetStreamAsync($"https://github.com/dchatel/CouchPotato/releases/download/{lastVersion}/CouchPotato.zip");
            using var stream = new MemoryStream();
            s.Result.CopyTo(stream);

            var zip = new ZipArchive(stream);
            foreach (var entry in zip.Entries)
            {
                var maybeFile = Path.Combine(root, entry.Name);
                if (Path.Exists(maybeFile))
                    File.Move(maybeFile, $"{maybeFile}.old");
                entry.ExtractToFile(maybeFile);
            }

            TaskName = Loc.UpdatedRestartPlease;
            await Task.Delay(5000);
            App.Restart();
        }
    }

    private async Task Migrate_v0()
    {
        TaskName = Loc.DatabaseInitialization;
        TaskIsIndeterminate = true;
        if (_db.GetInfrastructure().GetService<IMigrator>() is IMigrator migrator)
        {
            await migrator.MigrateAsync("CouchPotato_v0");
        }
        else
        {
            throw new ApplicationException(Loc.DatabaseMigrationServiceNotFound);
        }

        await MigrateGenres();
        await MigratePersons();
        await MigrateVideos();
        await MigrateRoles();
        await MigrateSeasons();
        await MigrateEpisodes();

        TaskName = Loc.DatabaseSaving;
        TaskIsIndeterminate = true;
        await _db.SaveChangesAsync();
    }

    private async Task MigrateEpisodes()
    {
        TaskName = Loc.DatabaseImportEpisodes;
        TaskIsIndeterminate = false;
        TaskMaximum = _videlib.Episodes.Count();
        TaskProgression = 0;

        await Task.Run(() =>
        {
            foreach (var episode in _videlib.Episodes.ToArray())
            {
                if (_dseasons.TryGetValue((int)episode.SeasonId!, out Season? season))
                {
                    _db.Episodes.Add(new Episode
                    {
                        EpisodeNumber = (int)episode.EpisodeNumber!,
                        Name = episode.Name!.Normalize(),
                        Overview = episode.Overview?.Normalize(),
                        ImageUrl = episode.StillPath,
                        PersonalRating = episode.Rating is not null ? (int)episode.Rating : null,
                        TmdbId = episode.TmdbId,
                        TmdbRating = episode.TmdbRating,
                        TmdbRatingCount = episode.TmdbRatingCount,
                        Season = season,
                    });
                }
                TaskProgression++;
            }
        });
    }

    private async Task MigrateSeasons()
    {
        TaskName = Loc.DatabaseImportSeasons;
        TaskIsIndeterminate = false;
        TaskMaximum = _videlib.Seasons.Count();
        TaskProgression = 0;

        await Task.Run(() =>
        {
            foreach (var s in _videlib.Seasons.ToArray())
            {
                if (_dvideos.TryGetValue((int)s.FilmId!, out Video? video)
                    && video.Type == VideoType.TVShow)
                {
                    var season = new Season
                    {
                        SeasonNumber = (int)s.SeasonNumber!,
                        Name = s.Name!.Normalize(),
                        Overview = s.Overview?.Normalize(),
                        PosterUrl = s.PosterPath,
                        TVShow = video,
                    };
                    _dseasons[(int)s.SeasonId!] = season;
                    video.Seasons.Add(season);
                }
                TaskProgression++;
            }
        });
    }

    private async Task MigrateRoles()
    {
        TaskName = Loc.DatabaseImportRoles;
        TaskIsIndeterminate = false;
        TaskMaximum = _videlib.Casts.Count();
        TaskProgression = 0;

        await Task.Run(() =>
        {
            foreach (var cast in _videlib.Casts
                .Include(c => c.Person)
                .ToArray())
            {
                if (_dvideos.TryGetValue((int)cast.FilmId!, out Video? video)
                    && _dpersons.TryGetValue((int)cast.Person!.TmdbId!, out Person? person))
                {
                    if (person.Roles.Any(r => r.Video == video))
                        continue;

                    _db.Roles.Add(new Role
                    {
                        Video = video,
                        Person = person,
                        Characters = cast.Characters?.Normalize(),
                        Order = cast.Order ?? 0,
                    });
                }
                TaskProgression++;
            }
        });
    }

    private async Task MigrateVideos()
    {
        TaskName = Loc.DatabaseImportVideos;
        TaskIsIndeterminate = false;
        TaskMaximum = _videlib.Films.Count();
        TaskProgression = 0;

        //var ofd = new CommonOpenFileDialog
        //{
        //    Title = Loc.FileSystemSelectVidelibImageFolder,
        //    IsFolderPicker = true,
        //};
        var ofd = new OpenFolderDialog
        {
            Title = Loc.FileSystemSelectVidelibImageFolder,
            Multiselect = false,
        };
        string? imageFolder = null;
        //if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
        //{
        //    imageFolder = ofd.FileName;
        //}
        if (ofd.ShowDialog() ?? false)
        {
            imageFolder = ofd.FolderName;
        }
        Directory.CreateDirectory("Images");

        await Task.Run(() =>
        {
            foreach (var film in _videlib.Films
                .Include(f => f.Genres)
                .ToArray())
            {
                var title = film.Title?.Normalize() ?? Guid.NewGuid().ToString();

                Video video = new()
                {
                    Type = film.EType switch
                    {
                        "movie" => VideoType.Movie,
                        "tv" => VideoType.TVShow,
                        _ => VideoType.Unknown,
                    },
                    Title = title,
                    Tagline = film.Tagline?.Normalize(),
                    Overview = film.Overview?.Normalize(),
                    ReleaseDate = film.ReleaseDate,
                    Status = film.Status?.Normalize(),

                    Origin = film.Origin,
                    OriginalTitle = film.OriginalTitle?.Normalize(),
                    Version = film.Version?.Normalize(),

                    PhysicalStorage = film.Place?.Normalize(),
                    PhysicalStorageCode = film.Location?.Normalize(),
                    DigitalStorageCode = film.Disk?.Normalize(),
                    DigitalFileFormat = film.Format?.Normalize(),
                    DigitalResolution = film.Resolution?.Normalize(),

                    Runtime = film.Runtime,

                    PosterUrl = imageFolder is null ? null : film.PosterPath switch
                    {
                        null => null,
                        _ => $"Images/{Utils.GetValidFileName(title)}.poster.{Guid.NewGuid()}.jpg"
                    },
                    BackgroundUrl = imageFolder is null ? null : film.BackdropPath switch
                    {
                        null => null,
                        _ => $"Images/{Utils.GetValidFileName(title)}.background.{Guid.NewGuid()}.jpg"
                    },

                    PersonalRating = film.Rating,

                    TmdbId = film.TmdbId,
                    TmdbRating = film.TmdbRating,
                    TmdbRatingCount = film.TmdbRatingCount,
                };

                if (imageFolder is not null)
                {
                    if (film.PosterPath is not null)
                    {
                        var file = Path.Combine(imageFolder, film.PosterPath);
                        if (File.Exists(file))
                        {
                            Utils.ResizeImage(file, video.PosterUrl!, width: 200);
                        }
                    }
                    if (film.BackdropPath is not null)
                    {
                        var file = Path.Combine(imageFolder, film.BackdropPath);
                        if (File.Exists(file))
                        {
                            Utils.ResizeImage(file, video.BackgroundUrl!);
                        }
                    }
                }

                _dvideos[(int)film.FilmId!] = video;
                foreach (var g in film.Genres)
                {
                    if (g.GenreId is not null && _dgenres.TryGetValue((int)g.GenreId, out Genre? genre))
                        video.Genres.Add(genre);
                }
                _db.Videos.Add(video);
                TaskProgression++;
            }
        });
    }

    private async Task MigratePersons()
    {
        TaskName = Loc.DatabaseImportPersons;
        TaskIsIndeterminate = false;
        TaskMaximum = _videlib.Persons.Count();
        TaskProgression = 0;

        await Task.Run(() =>
        {
            foreach (var p in _videlib.Persons.ToArray())
            {
                if (_dpersons.ContainsKey((int)p.TmdbId!))
                    continue;

                var person = new Person
                {
                    Name = p.Name!.Normalize(),
                    PortraitUrl = p.ProfilePath,
                    TmdbId = p.TmdbId,
                };
                _dpersons[(int)p.TmdbId!] = person;
                _db.Persons.Add(person);
                TaskProgression++;
            }
        });
    }

    private async Task MigrateGenres()
    {
        TaskName = Loc.DatabaseImportGenres;
        TaskIsIndeterminate = false;
        TaskMaximum = _videlib.Genres.Count();
        TaskProgression = 0;

        await Task.Run(() =>
        {
            foreach (var g in _videlib.Genres.ToArray())
            {
                var genre = new Genre
                {
                    Fixed = (bool)g.NoChange!,
                    Name = g.Name!.Normalize(),
                };
                _dgenres[(int)g.GenreId!] = genre;
                _db.Genres.Add(genre);
                TaskProgression++;
            }
        });
    }
}
