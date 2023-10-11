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
    public static async Task<IEnumerable<Person>> SearchActors(string searchText)
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
        return people.ToArray();
    }
}
