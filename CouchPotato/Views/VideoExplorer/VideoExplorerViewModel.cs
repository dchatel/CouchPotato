using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using CouchPotato.DbModel;

namespace CouchPotato.Views.VideoExplorer;

public class VideoExplorerViewModel : ContentViewModel
{
    public VideoExplorerViewModel()
    {
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        SearchBox = new SearchBox();
        TitleBarRegion = SearchBox;
    }

    public ICommand SearchCommand { get; }
    public SearchBox SearchBox { get; }
    public IEnumerable<SearchResult>? SearchResults { get; set; }
    public SearchResult? SelectedResult { get; set; }

    public async Task SearchAsync()
    {
        using var db = new DataContext();

        SearchResults = await Task.Run(() => db.Videos
            .Where(video => video.Title.Contains(SearchBox.SearchText))
            .Select(video => new SearchResult(video))
            .ToArray());
    }
}

public class SearchBox
{
    public SearchBox()
    {
    }
    public string SearchText { get; set; } = "";
}