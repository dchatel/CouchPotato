using System;
using System.Collections.Generic;

using CouchPotato.DbModel.OtherDbModels.Videlib;

namespace CouchPotato.DbModel;

public class Movie : Video
{
    public Movie() : base() { }

    public Movie(Video video) : this()
    {
        Title = video.Title!.Normalize();
        Tagline = video.Tagline?.Normalize();
        Overview = video.Overview?.Normalize();
        ReleaseDate = video.ReleaseDate;
        Status = video.Status?.Normalize();

        Origin = video.Origin;
        OriginalTitle = video.OriginalTitle?.Normalize();
        Version = video.Version?.Normalize();

        PhysicalStorage = video.PhysicalStorage?.Normalize();
        PhysicalStorageCode = video.PhysicalStorageCode?.Normalize();
        DigitalStorageCode = video.DigitalStorageCode?.Normalize();
        DigitalFileFormat = video.DigitalFileFormat?.Normalize();
        DigitalResolution = video.DigitalResolution?.Normalize();

        PosterUrl = video.PosterUrl;
        BackgroundUrl = video.BackgroundUrl;
        PersonalRating = video.PersonalRating;

        TmdbId = video.TmdbId;
        TmdbRating = video.TmdbRating;
        TmdbRatingCount = video.TmdbRatingCount;
    }

    public int? Runtime { get; set; }
}