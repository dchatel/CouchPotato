using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;

namespace CouchPotato.DbModel;

public enum VideoType
{
    Unknown,
    Movie,
    TVShow,
}

public class Video
{
    public int Id { get; set; }
    public VideoType Type { get; set; }

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

    public int? Runtime { get; set; }

    public ICollection<Genre> Genres { get; set; } = new HashSet<Genre>();
    public ICollection<Role> Roles { get; set; } = new HashSet<Role>();
    public ICollection<Season> Seasons { get; set; } = new HashSet<Season>();

    public Video CopyFromDb(DataContext db)
    {
        var video = db.Videos
                    .Include(v => v.Genres)
                    .Include(v => v.Roles).ThenInclude(r => r.Person)
                    .Include(v => v.Seasons).ThenInclude(s => s.Episodes)
                    .Single(v => v.Id == Id);
        return video;
    }
}