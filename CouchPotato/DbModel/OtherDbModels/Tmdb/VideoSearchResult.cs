using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchPotato.DbModel.OtherDbModels.Tmdb;

public class VideoSearchResult
{
    public int TmdbId { get; init; }
    public required string MediaType { get; init; }
    public required string Title { get; init; }
    public int? Year { get; init; }
    public required string PosterPath { get; init; }
}
