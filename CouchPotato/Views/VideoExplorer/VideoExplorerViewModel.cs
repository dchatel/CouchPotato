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
    private SearchResultViewModel? selectedResult;

    public VideoExplorerViewModel()
    {
        SearchBox = new SearchBoxViewModel();
        SearchCommand = new AsyncRelayCommand(SearchBox.SearchAsync);
        TitleBarRegion = SearchBox;
    }

    public ICommand SearchCommand { get; }
    public SearchBoxViewModel SearchBox { get; }
    public IEnumerable<SearchResultViewModel>? SearchResults => SearchBox.SearchResults;
    public SearchResultViewModel? SelectedResult
    {
        get => selectedResult;
        set {
            if (selectedResult is not null)
                selectedResult.IsSelected = false;
            selectedResult = value;
            if (selectedResult is not null)
                selectedResult.IsSelected = true;
        }
    }

    //public async Task SearchAsync()
    //{
    //    using var db = new DataContext();

    //    SearchResults = await Task.Run(() => db.Videos
    //        .Where(video => video.Title.Contains(SearchBox.SearchText))
    //        .Select(video => SearchResultViewModel.Create(video))
    //        .ToArray());
    //}
}

public class SearchBoxViewModel
{
    private string searchText = "";

    public string SearchText
    {
        get => searchText;
        set {
            searchText = value;
            Task.Run(SearchAsync);
        }
    }
    public IEnumerable<SearchResultViewModel>? SearchResults { get; set; }

    public SearchBoxViewModel()
    {
    }

    public async Task SearchAsync()
    {
        using var db = new DataContext();

        SearchResults = await Task.Run(() => db.Videos
            .Where(video => video.Title.ToLower().Contains(SearchText.ToLower()))
            .Select(video => SearchResultViewModel.Create(video))
            .ToArray());
    }
}