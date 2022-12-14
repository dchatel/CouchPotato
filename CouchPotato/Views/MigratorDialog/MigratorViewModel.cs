using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using CouchPotato.DbModel;
using CouchPotato.Properties;
using CouchPotato.Views.DummyTestDialog;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAPICodePack.Dialogs;

using Localization = CouchPotato.Properties.Localization;

namespace CouchPotato.Views.MigratorDialog;

public class MigratorViewModel : ContentViewModel
{
    public string? TaskName { get; private set; }
    public bool TaskIsIndeterminate { get; private set; }
    public int TaskMaximum { get; private set; }
    public int TaskProgression { get; private set; }

    private readonly DataContext db;
    private readonly DbModel.OtherDbModels.Videlib.DataContext videlib;
    private readonly Dictionary<int, Genre> dgenres = new();
    private readonly Dictionary<int, Person> dpersons = new();
    private readonly Dictionary<int, Video> dvideos = new();
    private readonly Dictionary<int, Season> dseasons = new();

    public MigratorViewModel()
    {
        db = new();
        videlib = new();
    }

    private bool AreMigrationsPending()
    {
        return db.Database.GetPendingMigrations().Any();
    }

    protected override async void OnLoaded()
    {
#if DEBUG
        //db.Database.EnsureDeleted();
        //if (Directory.Exists("Images"))
        //    Directory.Delete("Images", recursive: true);
#endif
        await Task.Delay(2000);

        if (!AreMigrationsPending())
        {
            Close(true);
            return;
        }

        try
        {
            if (!db.Database.GetAppliedMigrations().Any())
            {
                try
                {
                    await Migrate_v0();
                }
                catch
                {
                    db.Database.EnsureDeleted();
                    throw;
                }
            }
            TaskName = Localization.DatabaseUpdating;
            TaskIsIndeterminate = true;
            await db.Database.MigrateAsync();
            Close(true);
        }
        catch
        {
            TaskName = Localization.DatabaseMigrationFailed;
            await Task.Delay(5000);
            App.Current.MainWindow.Close();
            Close(false);
        }
    }

    private async Task Migrate_v0()
    {
        TaskName = Localization.DatabaseInitialization;
        TaskIsIndeterminate = true;
        if (db.GetInfrastructure().GetService<IMigrator>() is IMigrator migrator)
        {
            await migrator.MigrateAsync("CouchPotato_v0");
        }
        else
        {
            throw new ApplicationException(Localization.DatabaseMigrationServiceNotFound);
        }

        await MigrateGenres();
        await MigratePersons();
        await MigrateVideos();
        await MigrateRoles();
        await MigrateSeasons();
        await MigrateEpisodes();

        TaskName = Localization.DatabaseSaving;
        TaskIsIndeterminate = true;
        await db.SaveChangesAsync();

        await MigrateImages();
    }

    private async Task MigrateEpisodes()
    {
        TaskName = Localization.DatabaseImportEpisodes;
        TaskIsIndeterminate = false;
        TaskMaximum = videlib.Episodes.Count();
        TaskProgression = 0;

        await Task.Run(() =>
        {
            foreach (var episode in videlib.Episodes.ToArray())
            {
                if (dseasons.TryGetValue((int)episode.SeasonId!, out Season? season))
                {
                    db.Episodes.Add(new Episode
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
        TaskName = Localization.DatabaseImportSeasons;
        TaskIsIndeterminate = false;
        TaskMaximum = videlib.Seasons.Count();
        TaskProgression = 0;

        await Task.Run(() =>
        {
            foreach (var s in videlib.Seasons.ToArray())
            {
                if (dvideos.TryGetValue((int)s.FilmId!, out Video? video)
                    && video is TVShow tvShow)
                {
                    var season = new Season
                    {
                        SeasonNumber = (int)s.SeasonNumber!,
                        Name = s.Name!.Normalize(),
                        Overview = s.Overview?.Normalize(),
                        PosterUrl = s.PosterPath,
                        TVShow = tvShow,
                    };
                    dseasons[(int)s.SeasonId!] = season;
                    tvShow.Seasons.Add(season);
                }
                TaskProgression++;
            }
        });
    }

    private async Task MigrateRoles()
    {
        TaskName = Localization.DatabaseImportRoles;
        TaskIsIndeterminate = false;
        TaskMaximum = videlib.Casts.Count();
        TaskProgression = 0;

        await Task.Run(() =>
        {
            foreach (var cast in videlib.Casts
                .Include(c => c.Person)
                .ToArray())
            {
                if (dvideos.TryGetValue((int)cast.FilmId!, out Video? video)
                    && dpersons.TryGetValue((int)cast.Person!.TmdbId!, out Person? person))
                {
                    if (person.Roles.Any(r => r.Video == video))
                        continue;

                    db.Roles.Add(new Role
                    {
                        Video = video,
                        Person = person,
                        Characters = cast.Characters?.Normalize(),
                        Order = cast.Order ?? 0,
                    });
                }
            }
        });
    }

    private async Task MigrateVideos()
    {
        TaskName = Localization.DatabaseImportVideos;
        TaskIsIndeterminate = false;
        TaskMaximum = videlib.Films.Count();
        TaskProgression = 0;

        await Task.Run(() =>
        {
            foreach (var film in videlib.Films.ToArray())
            {
                Video video = new()
                {
                    Title = film.Title!.Normalize(),
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

                    PosterUrl = film.PosterPath is not null ? $"Images/{film.PosterPath}" : null,
                    BackgroundUrl = film.BackdropPath is not null ? $"Images/{film.BackdropPath}" : null,

                    PersonalRating = film.Rating,

                    TmdbId = film.TmdbId,
                    TmdbRating = film.TmdbRating,
                    TmdbRatingCount = film.TmdbRatingCount,
                };

                switch (film.EType)
                {
                    case "movie":
                        video = new Movie(video)
                        {
                            Runtime = film.Runtime,
                        };
                        break;
                    case "tv":
                        video = new TVShow(video);
                        break;
                    default:
                        break;
                }

                dvideos[(int)film.FilmId!] = video;
                foreach (var g in film.Genres)
                {
                    if (dgenres.TryGetValue((int)g.GenreId!, out Genre? genre))
                        video.Genres.Add(genre);
                }
                db.Videos.Add(video);
                TaskProgression++;
            }
        });
    }

    private async Task MigrateImages()
    {
        var ofd = new CommonOpenFileDialog
        {
            Title = Localization.FileSystemSelectVidelibImageFolder,
            IsFolderPicker = true,
        };
        if (ofd.ShowDialog() != CommonFileDialogResult.Ok)
            return;

        var files = Directory.GetFiles(ofd.FileName);

        TaskName = Localization.DatabaseImportImages;
        TaskIsIndeterminate = false;
        TaskMaximum = files.Length;
        TaskProgression = 0;
        await Task.Run(() =>
        {
            Directory.CreateDirectory("Images");
            foreach (var file in files)
            {
                var newfile = $"Images/{Path.GetFileName(file)}";
                File.Copy(file, newfile);
                File.SetAttributes(newfile, FileAttributes.Normal);
                TaskProgression++;
            }
        });
    }

    private async Task MigratePersons()
    {
        TaskName = Localization.DatabaseImportPersons;
        TaskIsIndeterminate = false;
        TaskMaximum = videlib.Persons.Count();
        TaskProgression = 0;

        await Task.Run(() =>
        {
            foreach (var p in videlib.Persons.ToArray())
            {
                if (dpersons.ContainsKey((int)p.TmdbId!))
                    continue;

                var person = new Person
                {
                    Name = p.Name!.Normalize(),
                    PortraitUrl = p.ProfilePath,
                    TmdbId = p.TmdbId,
                };
                dpersons[(int)p.TmdbId!] = person;
                db.Persons.Add(person);
                TaskProgression++;
            }
        });
    }

    private async Task MigrateGenres()
    {
        TaskName = Localization.DatabaseImportGenres;
        TaskIsIndeterminate = false;
        TaskMaximum = videlib.Genres.Count();
        TaskProgression = 0;

        await Task.Run(() =>
        {
            foreach (var g in videlib.Genres.ToArray())
            {
                var genre = new Genre
                {
                    Fixed = (bool)g.NoChange!,
                    Name = g.Name!.Normalize(),
                };
                dgenres[(int)g.GenreId!] = genre;
                db.Genres.Add(genre);
                TaskProgression++;
            }
        });
    }
}
