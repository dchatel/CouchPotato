using CouchPotato.DbModel;

namespace CouchPotato.Views.VideoExplorer;

public class SearchResultViewModel
{
    private bool isSelected;

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

    public SearchResultViewModel(Video video)
    {
        Video = video;
    }

    public bool IsSelected
    {
        get => isSelected;
        set {
            LoadData();
            isSelected = value;
        }
    }

    protected virtual void LoadData()
    {
        using var db = new DataContext();
        db.Attach(Video);
        db.Entry(Video).Collection(v => v.Genres).LoadAsync();
        db.Entry(Video).Collection(v => v.Roles).LoadAsync();
        foreach (var role in Video.Roles)
            db.Entry(role).Reference(r => r.Person).LoadAsync();
    }

    public Video Video { get; }
}

public class MovieSearchResultViewModel : SearchResultViewModel
{
    public MovieSearchResultViewModel(Video video) : base(video) { }
}

public class TVShowSearchResultViewModel : SearchResultViewModel
{
    public TVShowSearchResultViewModel(Video video) : base(video) { }

    public TVShow TVShow => (TVShow)Video;

    protected override void LoadData()
    {
        using var db = new DataContext();
        db.Attach(Video);
        db.Entry(Video).Collection(v => v.Genres).LoadAsync();
        db.Entry(Video).Collection(v => v.Roles).LoadAsync();
        foreach (var role in Video.Roles)
            db.Entry(role).Reference(r => r.Person).LoadAsync();
        if (Video is TVShow tv)
        {
            db.Entry(tv).Collection(v => v.Seasons).LoadAsync();
            foreach (var season in tv.Seasons)
                db.Entry(season).Collection(s => s.Episodes).LoadAsync();
        }
    }
}