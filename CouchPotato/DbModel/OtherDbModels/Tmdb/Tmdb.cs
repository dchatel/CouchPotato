using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CouchPotato.Properties;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

using TMDbLib.Client;
using TMDbLib.Objects.Movies;

namespace CouchPotato.DbModel.OtherDbModels.Tmdb;

public class Tmdb
{
    public static async Task<IEnumerable<Person>> SearchActors(DataContext db, string searchText, IEnumerable<Person> excludedPeople)
    {
        using var tmdb = new TMDbClient(Config.Default.TMDbAPIKey);

        var results = await tmdb.SearchPersonAsync(searchText, Config.Default.Language);
        var people = await Task.Run(() => from tp in results.Results
                                          join p in db.Persons on tp.Id equals p.TmdbId into gp
                                          from sp in gp.DefaultIfEmpty()
                                          select sp ?? new Person
                                          {
                                              Name = tp.Name,
                                              PortraitUrl = tp.ProfilePath,
                                              TmdbId = tp.Id,
                                          });
        people = people.Where(p => excludedPeople.All(e => p.TmdbId != e.TmdbId));
        return people.ToArray();
    }

    public static async Task<IEnumerable<VideoSearchResult>> SearchVideo(string mediaType, string title, int year)
    {
        using var tmdb = new TMDbClient(Config.Default.TMDbAPIKey);

        var results = mediaType switch
        {
            "movie" => (await tmdb.SearchMovieAsync(title, Config.Default.Language, year: year)).Results.Select(m => new VideoSearchResult
            {
                TmdbId = m.Id,
                MediaType = "movie",
                Title = m.Title,
                PosterPath = m.PosterPath,
                Year = m.ReleaseDate?.Year,
            }),
            "tv" => (await tmdb.SearchTvShowAsync(title, Config.Default.Language, firstAirDateYear: year)).Results.Select(s => new VideoSearchResult
            {
                TmdbId = s.Id,
                MediaType = "tv",
                Title = s.Name,
                PosterPath = s.PosterPath,
                Year = s.FirstAirDate?.Year,
            }),
            "unknown" => (await SearchVideo("movie", title, year)).Union(await SearchVideo("tv", title, year)),
            _ => throw new NotImplementedException()
        };

        return results;
    }

    public static async Task<Video> GetVideo(DataContext db, int tmdbId, string mediaType)
    {
        var video = mediaType switch
        {
            "movie" => await GetMovie(db, tmdbId),
            "tv" => await GetTVShow(db, tmdbId),
            _ => throw new NotImplementedException(),
        };

        return video;
    }

    public static async Task<Video> GetMovie(DataContext db, int tmdbId)
    {
        using var tmdb = new TMDbClient(Config.Default.TMDbAPIKey);
        var tmdbMovie = await tmdb.GetMovieAsync(tmdbId, language: Config.Default.Language, extraMethods: TMDbLib.Objects.Movies.MovieMethods.Credits);

        var movie = new Video
        {
            Type = VideoType.Movie,

            Title = tmdbMovie.Title,
            Tagline = tmdbMovie.Tagline,
            Overview = tmdbMovie.Overview,
            ReleaseDate = tmdbMovie.ReleaseDate,
            Status = tmdbMovie.Status,

            Origin = tmdbMovie.OriginalLanguage,
            OriginalTitle = tmdbMovie.OriginalTitle,

            PosterUrl = tmdbMovie.PosterPath,
            BackgroundUrl = tmdbMovie.BackdropPath,

            TmdbId = tmdbMovie.Id,
            TmdbRating = tmdbMovie.VoteAverage,
            TmdbRatingCount = tmdbMovie.VoteCount,

            Runtime = tmdbMovie.Runtime,
        };

        SyncGenres(db, movie, tmdbMovie.Genres);
        SyncRoles(db, movie, tmdbMovie.Credits.Cast);

        return movie;
    }


    public static async Task<Video> GetTVShow(DataContext db, int tmdbId)
    {
        using var tmdb = new TMDbClient(Config.Default.TMDbAPIKey);
        var tmdbTVShow = await tmdb.GetTvShowAsync(tmdbId, language: Config.Default.Language,
            extraMethods: TMDbLib.Objects.TvShows.TvShowMethods.Credits);

        var tvShow = new Video
        {
            Type = VideoType.TVShow,

            Title = tmdbTVShow.Name,
            Tagline = tmdbTVShow.Tagline,
            Overview = tmdbTVShow.Overview,
            ReleaseDate = tmdbTVShow.FirstAirDate,
            Status = tmdbTVShow.Status,

            Origin = tmdbTVShow.OriginalLanguage,
            OriginalTitle = tmdbTVShow.OriginalName,

            PosterUrl = tmdbTVShow.PosterPath,
            BackgroundUrl = tmdbTVShow.BackdropPath,

            TmdbId = tmdbTVShow.Id,
            TmdbRating = tmdbTVShow.VoteAverage,
            TmdbRatingCount = tmdbTVShow.VoteCount,

            Runtime = (int)tmdbTVShow.EpisodeRunTime.Average(),
        };

        SyncGenres(db, tvShow, tmdbTVShow.Genres);
        SyncRoles(db, tvShow, tmdbTVShow.Credits.Cast);

        foreach (var tmdbSeason in tmdbTVShow.Seasons)
        {
            var season = new Season
            {
                TmdbId = tmdbTVShow.Id,
                Name = tmdbSeason.Name,
                Overview = tmdbSeason.Overview,
                SeasonNumber = tmdbSeason.SeasonNumber,
                PosterUrl = tmdbSeason.PosterPath,
                TVShow = tvShow,
            };
            tvShow.Seasons.Add(season);
            var tmdbSeasonInfo = await tmdb.GetTvSeasonAsync(tmdbId, season.SeasonNumber, language: Config.Default.Language);
            foreach (var tmdbEpisode in tmdbSeasonInfo.Episodes)
            {
                var episode = new Episode
                {
                    EpisodeNumber = tmdbEpisode.EpisodeNumber,
                    Name = tmdbEpisode.Name,
                    Overview = tmdbEpisode.Overview,
                    Runtime = tmdbEpisode.Runtime,
                    ImageUrl = tmdbEpisode.StillPath,
                    TmdbId = tmdbEpisode.Id,
                    TmdbRating = tmdbEpisode.VoteAverage,
                    TmdbRatingCount = tmdbEpisode.VoteCount,
                };
                season.Episodes.Add(episode);
            }
        }

        return tvShow;
    }

    private static void SyncRoles(DataContext db, Video movie, IEnumerable<dynamic> tmdbCasts)
    {
        var roles = from tmdbCast in tmdbCasts
                    join person in db.Persons on tmdbCast.Id equals person.TmdbId into gp
                    from sp in gp.DefaultIfEmpty()
                    select new Role
                    {
                        Video = movie,
                        Person = sp ?? new Person
                        {
                            Name = tmdbCast.Name,
                            PortraitUrl = tmdbCast.ProfilePath,
                            TmdbId = tmdbCast.Id,
                        },
                        Order = tmdbCast.Order,
                        Characters = tmdbCast.Character,
                    };
        foreach (var role in roles)
        {
            movie.Roles.Add(role);
        }
    }

    private static void SyncGenres(DataContext db, Video movie, IEnumerable<TMDbLib.Objects.General.Genre> tmdbGenres)
    {
        var genres = from tmdbgenre in tmdbGenres
                     from genre in db.Genres
                     where tmdbgenre.Name.Contains(genre.Name)
                     select genre;
        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }
    }
}
