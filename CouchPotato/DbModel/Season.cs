using System.Collections.Generic;

namespace CouchPotato.DbModel;

public class Season
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string? Overview { get; set; }
    public string? PosterUrl { get; set; }
    public int SeasonNumber { get; set; }
    public int? TmdbId { get; set; }

    public ICollection<Episode> Episodes { get; set; } = new HashSet<Episode>();
    public Video TVShow { get; set; } = null!;
}