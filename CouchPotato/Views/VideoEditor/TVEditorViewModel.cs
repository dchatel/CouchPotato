using System.Collections.ObjectModel;
using System.Linq;

using CouchPotato.DbModel;

namespace CouchPotato.Views.VideoEditor;

public class TVEditorViewModel : VideoEditorViewModel
{
    public TVShow TVShow { get; }
    public ObservableCollection<SeasonViewModel> Seasons { get; }

    public TVEditorViewModel(DataContext db, int videoId) : base(db, videoId)
    {
        TVShow = (TVShow)Video;
        _db.Entry(TVShow).Collection(tv => tv.Seasons).Load();

        Seasons = new(TVShow.Seasons.Select(s => new SeasonViewModel(s)));
    }
}

public class SeasonViewModel
{
    public Season Season { get; }

    public SeasonViewModel(Season season)
    {
        Season = season;
    }
}