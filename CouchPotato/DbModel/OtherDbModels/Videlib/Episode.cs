using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CouchPotato.DbModel.OtherDbModels.Videlib;

[Table("Episodes")]
public partial class Episode
{
    public DateTime? AirDate { get; set; }
    public int? EpisodeId { get; set; }
    public int? EpisodeNumber { get; set; }
    public string? Name { get; set; }
    public string? Overview { get; set; }
    public double? Rating { get; set; }
    public virtual Season? Season { get; set; }
    public int? SeasonId { get; set; }
    public string? StillPath { get; set; }
    public int? TmdbId { get; set; }
    public double? TmdbRating { get; set; }
    public int? TmdbRatingCount { get; set; }
}