using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using CouchPotato.DbModel;
using CouchPotato.Views.VideoEditor;

using Microsoft.EntityFrameworkCore;

namespace CouchPotato.Views.VideoExplorer;

public class VideoExplorerViewModel : ContentViewModel
{
    public VideoExplorerViewModel() : base(autoClose: false)
    {
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        EditCommand = new AsyncRelayCommand(Edit);
        IsSearching = false;
    }

    public ICommand EditCommand { get; }
    public ICommand SearchCommand { get; }
    public IEnumerable<SearchResultViewModel>? SearchResults { get; set; }
    public bool IsSearching { get; set; }

    private SearchResultViewModel? selectedResult;
    public SearchResultViewModel? SelectedResult
    {
        get => selectedResult;
        set {
            selectedResult = value;
            if (selectedResult is not null)
                Task.Run(selectedResult.LoadData);
        }
    }

    private string searchText = "";
    public string SearchText
    {
        get => searchText;
        set {
            searchText = value;
            Task.Run(SearchAsync);
        }
    }

    private async Task Edit()
    {
        if (SelectedResult is null) return;

        var videoEditor = new VideoEditorViewModel(SelectedResult.Video.Id);
        if (await videoEditor.Show())
        {
            SelectedResult.Video = videoEditor.Video;
        }
    }

    private async Task SearchAsync()
    {
        IsSearching = true;
        using var db = new DataContext();

        SearchResults = await Task.Run(() => db.Videos
            .Where(video => video.Title.ToLower().Contains(SearchText.ToLower()))
            .Select(video => SearchResultViewModel.Create(video))
            .ToArray());
        SelectedResult = SearchResults.FirstOrDefault();
        IsSearching = false;
    }
}