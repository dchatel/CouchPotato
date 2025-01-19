using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using CouchPotato.DbModel;

using Microsoft.EntityFrameworkCore;

namespace CouchPotato.Views.VideoExplorer;


public partial class VideoViewerViewModel : ObservableObject
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

    [ObservableProperty]
    public bool _isLoaded;
    [ObservableProperty]
    public bool _isLoading;

    public Video Video
    {
        get => _video;
        set => SetProperty(ref _video, value);
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

    public virtual async Task LoadData()
    {
        if (IsLoaded || IsLoading) return;
        
        IsLoading = true;
        
        using var db = new DataContext();
        Video = await db.Videos
            .Include(v => v.Genres)
            .Include(v => v.Roles).ThenInclude(r => r.Person)
            .Include(v => v.Seasons).ThenInclude(s => s.Episodes)
            .SingleAsync(v => v.Id == Video.Id);

        IsLoading = false;
        IsLoaded = true;
    }
}

public partial class MovieViewerViewModel : VideoViewerViewModel
{
    public MovieViewerViewModel(Video video) : base(video) { }
}

public partial class TVShowViewerViewModel : VideoViewerViewModel
{
    [ObservableProperty]
    private object _currentPage = null!;

    public TVShowViewerViewModel(Video video) : base(video) { }

    public IEnumerable<object> Pages { get; set; } = null!;

    public override async Task LoadData()
    {
        await base.LoadData();

        var list = new List<object>
            {
                Video
            };
        list.AddRange(Video.Seasons);
        Pages = list;
        CurrentPage = Pages.First();
    }
}
