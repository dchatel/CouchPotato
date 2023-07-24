using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using CouchPotato.DbModel;
using CouchPotato.Views.VideoEditor;

namespace CouchPotato.Views.VideoExplorer;

public class VideoExplorerViewModel : ContentViewModel
{
    private SearchResultViewModel? selectedResult;

    public VideoExplorerViewModel()
    {
        Toolbar = new VideoExplorerToolbarViewModel(this);
        SearchCommand = new AsyncRelayCommand(Toolbar.SearchAsync);
        TitleBarRegion = Toolbar;
    }

    public ICommand SearchCommand { get; }
    public VideoExplorerToolbarViewModel Toolbar { get; }
    public IEnumerable<SearchResultViewModel>? SearchResults => Toolbar.SearchResults;
    public SearchResultViewModel? SelectedResult
    {
        get => selectedResult;
        set {
            if (selectedResult is not null)
                selectedResult.IsSelected = false;
            selectedResult = value;
            if (selectedResult is not null)
                selectedResult.IsSelected = true;
            base.OnPropertyChanged(nameof(SelectedResult));
        }
    }
}

public class VideoExplorerToolbarViewModel
{
    private readonly VideoExplorerViewModel videoExplorer;

    private string searchText = "";
    public string SearchText
    {
        get => searchText;
        set {
            searchText = value;
            Task.Run(SearchAsync);
        }
    }
    public ICommand EditCommand { get; }
    public IEnumerable<SearchResultViewModel>? SearchResults { get; set; }

    public VideoExplorerToolbarViewModel(VideoExplorerViewModel videoExplorer)
    {
        this.videoExplorer = videoExplorer;
        EditCommand = new AsyncRelayCommand(Edit);
    }

    private async Task Edit()
    {
        if (videoExplorer.SelectedResult is null) return;

        using var db = new DataContext();
        var video = db.Videos
            .Where(video => video.Id == videoExplorer.SelectedResult.Video.Id)
            .Single();
        var videoEditor = new VideoEditorViewModel(video);
        if (await videoEditor.Show())
        {
            await db.SaveChangesAsync();
            videoExplorer.SelectedResult.Video = video;
        }
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