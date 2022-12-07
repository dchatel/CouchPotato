using System;
using System.Collections.Generic;

namespace CouchPotato.DbModel.OtherDbModels.Videlib;

public partial class Film
{
    public Film()
    {
        Casts = new HashSet<Cast>();
        Seasons = new HashSet<Season>();
        Genres = new HashSet<Genre>();
    }

    // Tmdb
    public string? BackdropPath { get; set; }

    public virtual ICollection<Cast> Casts { get; set; }
    public string? Disk { get; set; }

    // TV
    public int? EpisodeCount { get; set; }

    public string? EType { get; set; }
    public int? FilmId { get; set; }
    public string? Format { get; set; }
    public virtual ICollection<Genre> Genres { get; set; }
    public DateTime? LastAirDate { get; set; }
    public string? Location { get; set; }
    public string? Origin { get; set; }
    public string? OriginalLanguage { get; set; }
    public string? OriginalTitle { get; set; }
    public string? Overview { get; set; }
    public string? Place { get; set; }
    public string? PosterPath { get; set; }
    public int? Rating { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? Resolution { get; set; }
    public int? Runtime { get; set; }
    public virtual Saga? Saga { get; set; }

    // Movie
    public int? SagaId { get; set; }

    public int? SeasonCount { get; set; }
    public virtual ICollection<Season> Seasons { get; set; }
    public string? Status { get; set; }
    public string? Tagline { get; set; }
    public string? Title { get; set; }
    public int? TmdbId { get; set; }
    public double? TmdbRating { get; set; }
    public int? TmdbRatingCount { get; set; }
    public string? Version { get; set; }
}