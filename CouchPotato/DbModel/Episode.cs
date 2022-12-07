namespace CouchPotato.DbModel;

public class Episode
{
    public int Id { get; set; }
    public int EpisodeNumber { get; set; }
    public string Name { get; set; } = null!;
    public string? Overview { get; set; }
    public int? Runtime { get; set; }
    public string? ImageUrl { get; set; }

    public string? DigitalFileFormat { get; set; }
    public string? DigitalResolution { get; set; }

    public int? PersonalRating { get; set; }

    public int? TmdbId { get; set; }
    public double? TmdbRating { get; set; }
    public int? TmdbRatingCount { get; set; }

    public Season Season { get; set; } = null!;
}