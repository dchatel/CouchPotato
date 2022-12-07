using System.Collections.Generic;

namespace CouchPotato.DbModel.OtherDbModels.Videlib;

public partial class Season
{
    public Season()
    {
        Episodes = new HashSet<Episode>();
    }

    public int? EpisodeCount { get; set; }
    public virtual ICollection<Episode> Episodes { get; set; }
    public virtual Film? Film { get; set; }
    public int? FilmId { get; set; }
    public string? Name { get; set; }
    public string? Overview { get; set; }
    public string? PosterPath { get; set; }
    public int? SeasonId { get; set; }
    public int? SeasonNumber { get; set; }
    public int? TmdbId { get; set; }
}