using System.Collections.Generic;

namespace CouchPotato.DbModel;

public class Season
{
    public Season()
    {
        Episodes = new HashSet<Episode>();
    }

    public ICollection<Episode> Episodes { get; set; }
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string? Overview { get; set; }
    public string? PosterUrl { get; set; }
    public int SeasonNumber { get; set; }
    public int? TmdbId { get; set; }

    public TVShow TVShow { get; set; } = null!;
}