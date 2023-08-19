using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using CouchPotato.DbModel;
using CouchPotato.Views.OkDialog;

using PostSharp.Patterns.Model;

namespace CouchPotato.Views.VideoExplorer;

public class SearchResultViewModel
{
    public static SearchResultViewModel Create(Video video)
    {
        SearchResultViewModel result = video switch
        {
            Movie => new MovieSearchResultViewModel(video),
            TVShow => new TVShowSearchResultViewModel(video),
            _ => new SearchResultViewModel(video)
        };
        return result;
    }

    //private bool isSelected;

    public Video Video { get; set; }

    protected SearchResultViewModel(Video video)
    {
        Video = video;
    }

    //public bool IsSelected
    //{
    //    get => isSelected;
    //    set {
    //        isSelected = value;
    //        if (isSelected)
    //        {
    //            LoadData();
    //        }
    //    }
    //}

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
        using var db = new DataContext();
        db.Attach(Video);
        await db.Entry(Video).Collection(v => v.Genres).LoadAsync();
        await db.Entry(Video).Collection(v => v.Roles).LoadAsync();
        foreach (var role in Video.Roles)
            await db.Entry(role).Reference(r => r.Person).LoadAsync();
    }
}

public class MovieSearchResultViewModel : SearchResultViewModel
{
    public MovieSearchResultViewModel(Video video) : base(video) { }
}

public class TVShowSearchResultViewModel : SearchResultViewModel
{
    public TVShowSearchResultViewModel(Video video) : base(video) { }

    public TVShow TVShow => (TVShow)Video;
    public IEnumerable<object> Pages { get; set; } = null!;
    public object CurrentPage { get; set; } = null!;

    public override async Task LoadData()
    {
        using var db = new DataContext();
        db.Attach(Video);
        await db.Entry(Video).Collection(v => v.Genres).LoadAsync();
        await db.Entry(Video).Collection(v => v.Roles).LoadAsync();
        foreach (var role in Video.Roles)
            await db.Entry(role).Reference(r => r.Person).LoadAsync();
        if (Video is TVShow tv)
        {
            await db.Entry(tv).Collection(v => v.Seasons).LoadAsync();
            foreach (var season in tv.Seasons)
                await db.Entry(season).Collection(s => s.Episodes).LoadAsync();

            var list = new List<object>
            {
                tv
            };
            list.AddRange(tv.Seasons.Select(s => new SeasonViewModel(s)));
            Pages = list;
            CurrentPage = Pages.First();
        }
    }
}

public class SeasonViewModel
{
    private IEnumerable<EpisodeViewModel> episodes = null!;

    public SeasonViewModel(Season season)
    {
        Season = season;
    }

    public Season Season { get; }
    [SafeForDependencyAnalysis]
    public IEnumerable<EpisodeViewModel> Episodes
    {
        get {
            episodes ??= Season.Episodes.Select(e => new EpisodeViewModel(e)).ToList();
            return episodes;
        }
        set => episodes = value;
    }
}

public class EpisodeViewModel : ContentViewModel
{
    public Episode Episode { get; init; } = null!;
    public ICommand Zoom { get; }
    public bool Zoomed { get; set; }

    public EpisodeViewModel(Episode episode) : base(autoClose: true)
    {
        Episode = episode;
        Zoom = new AsyncRelayCommand(async () =>
        {
            Zoomed = true;
            await Show();
            Zoomed = false;
        });
    }
}
