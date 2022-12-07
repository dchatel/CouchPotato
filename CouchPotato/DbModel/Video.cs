using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchPotato.DbModel;

public class Video
{
    public Video()
    {
        Genres = new HashSet<Genre>();
        Roles = new HashSet<Role>();
    }

    public int Id { get; set; }

    public string Title { get; set; } = null!;
    public string? Tagline { get; set; }
    public string? Overview { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? Status { get; set; }

    public string? Origin { get; set; }
    public string? OriginalTitle { get; set; }
    public string? Version { get; set; }

    public string? PhysicalStorage { get; set; }
    public string? PhysicalStorageCode { get; set; }
    public string? DigitalStorageCode { get; set; }
    public string? DigitalFileFormat { get; set; }
    public string? DigitalResolution { get; set; }

    public string? PosterUrl { get; set; }
    public string? BackgroundUrl { get; set; }

    public int? PersonalRating { get; set; }

    public int? TmdbId { get; set; }
    public double? TmdbRating { get; set; }
    public int? TmdbRatingCount { get; set; }

    public ICollection<Genre> Genres { get; set; }
    public ICollection<Role> Roles { get; set; }
}