using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CouchPotato.Properties;

using TMDbLib.Client;

namespace CouchPotato.DbModel.OtherDbModels.Tmdb;

public class Tmdb
{
    public static async Task<IEnumerable<Person>> SearchActors(string searchText, IEnumerable<Person> excludedPeople)
    {
        using var tmdb = new TMDbClient(Config.Default.TMDbAPIKey);
        using var db = new DataContext();

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
}
