using System.Collections.Generic;
using System.Linq;

using CouchPotato.DbModel;

namespace CouchPotato.Views.VideoExplorer;

public class VideoViewerViewModel
{
    private Video _video;

    public static VideoViewerViewModel Create(Video video)
    {
        VideoViewerViewModel result = video.Type switch
        {
            VideoType.Movie => new MovieViewerViewModel(video),
            VideoType.TVShow => new TVShowViewerViewModel(video),
            _ => new VideoViewerViewModel(video)
        };
        return result;
    }

    public Video Video
    {
        get => _video;
        set {
            _video = value;
            LoadData();
        }
    }

    protected VideoViewerViewModel(Video video)
    {
        _video = video;
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
        if (Video is Video tv)
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
