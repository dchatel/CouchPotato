using System;
using System.Collections.Generic;
using System.Linq;

using CouchPotato.DbModel;

namespace CouchPotato.Views.VideoEditor;

public class VideoSyncViewModel : ContentViewModel
{
    public VideoType Type { get; }
    public PropertyUpdate<string> Title { get; }
    public PropertyUpdate<string?> Tagline { get; }
    public PropertyUpdate<string?> Overview { get; }
    public PropertyUpdate<DateTime?> ReleaseDate { get; }
    public PropertyUpdate<string?> Status { get; }

    public PropertyUpdate<string?> Origin { get; }
    public PropertyUpdate<string?> OriginalTitle { get; }
    public PropertyUpdate<string?> Version { get; }

    public PropertyUpdate<string?> PosterUrl { get; }
    public PropertyUpdate<string?> BackgroundUrl { get; }

    public PropertyUpdate<int?> TmdbId { get; }
    public PropertyUpdate<double?> TmdbRating { get; }
    public PropertyUpdate<int?> TmdbRatingCount { get; }

    public PropertyUpdate<int?> Runtime { get; }

    public PropertyUpdate<ICollection<Genre>> Genres { get; }
    public PropertyUpdate<ICollection<Role>> Roles { get; }
    public ICollection<Season> Seasons { get; }
    public bool ReplaceSeasons { get; set; }

    public VideoSyncViewModel(Video video, Video selectedVideo) : base(autoClose: false)
    {
        Type = video.Type;

        Title = new(video.Title, selectedVideo.Title);
        Tagline = new(video.Tagline, selectedVideo.Tagline);
        Overview = new(video.Overview, selectedVideo.Overview);
        ReleaseDate = new(video.ReleaseDate, selectedVideo.ReleaseDate);
        Status = new(video.Status, selectedVideo.Status);
        Origin = new(video.Origin, selectedVideo.Origin);
        OriginalTitle = new(video.OriginalTitle, selectedVideo.OriginalTitle);
        Version = new(video.Version, selectedVideo.Version);
        PosterUrl = new(video.PosterUrl, selectedVideo.PosterUrl);
        BackgroundUrl = new(video.BackgroundUrl, selectedVideo.BackgroundUrl);
        TmdbId = new(video.TmdbId, selectedVideo.TmdbId);
        TmdbRating = new(video.TmdbRating, selectedVideo.TmdbRating);
        TmdbRatingCount = new(video.TmdbRatingCount, selectedVideo.TmdbRatingCount);
        Runtime = new(video.Runtime, selectedVideo.Runtime);

        Genres = new(video.Genres, selectedVideo.Genres);
        Roles = new(video.Roles, selectedVideo.Roles);

        Seasons = selectedVideo.Seasons;
        ReplaceSeasons = false;
    }
}