﻿using System.Collections.Generic;
using System.Linq;

using CouchPotato.DbModel;

namespace CouchPotato.Views.VideoExplorer;

public class VideoViewerViewModel
{
    public static VideoViewerViewModel Create(Video video)
    {
        VideoViewerViewModel result = video switch
        {
            Movie => new MovieViewerViewModel(video),
            TVShow => new TVShowViewerViewModel(video),
            _ => new VideoViewerViewModel(video)
        };
        return result;
    }

    public Video Video { get; set; }

    protected VideoViewerViewModel(Video video)
    {
        Video = video;
    }

    public int PersonalRating
    {
        get => Video?.PersonalRating ?? 0;
        set {
            if (Video is null) return;

            using var db = new DataContext();
            db.Attach(Video);
            Video.PersonalRating = value;
            db.SaveChangesAsync();
        }
    }

    public virtual void LoadData()
    {
        using var db = new DataContext();
        db.Attach(Video);
        db.Entry(Video).Collection(v => v.Genres).Load();
        db.Entry(Video).Collection(v => v.Roles).Load();
        foreach (var role in Video.Roles)
            db.Entry(role).Reference(r => r.Person).Load();
    }
}

public class MovieViewerViewModel : VideoViewerViewModel
{
    public MovieViewerViewModel(Video video) : base(video) { }
}

public class TVShowViewerViewModel : VideoViewerViewModel
{
    public TVShowViewerViewModel(Video video) : base(video) { }

    public TVShow TVShow => (TVShow)Video;
    public IEnumerable<object> Pages { get; set; } = null!;
    public object CurrentPage { get; set; } = null!;

    public override void LoadData()
    {
        using var db = new DataContext();
        db.Attach(Video);
        db.Entry(Video).Collection(v => v.Genres).Load();
        db.Entry(Video).Collection(v => v.Roles).Load();
        foreach (var role in Video.Roles)
            db.Entry(role).Reference(r => r.Person).Load();
        if (Video is TVShow tv)
        {
            db.Entry(tv).Collection(v => v.Seasons).Load();
            foreach (var season in tv.Seasons)
                db.Entry(season).Collection(s => s.Episodes).Load();

            var list = new List<object>
            {
                tv
            };
            list.AddRange(tv.Seasons);
            Pages = list;
            CurrentPage = Pages.First();
        }
    }
}
