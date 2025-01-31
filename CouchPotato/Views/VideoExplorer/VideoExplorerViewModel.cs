using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using CouchPotato.DbModel;
using CouchPotato.Views.VideoEditor;

using Microsoft.EntityFrameworkCore;

namespace CouchPotato.Views.VideoExplorer;

public partial class VideoExplorerViewModel : ContentViewModel
{
    [ObservableProperty]
    private bool _isAdvancedSearchDialogOpened;
    [ObservableProperty]
    private bool _isSearching;
    [ObservableProperty]
    private ObservableCollection<VideoSearchResultViewModel>? _searchResults;
    [ObservableProperty]
    private string _title;
    [ObservableProperty]
    private string _actors;
    [ObservableProperty]
    private string _roles;

    private VideoSearchResultViewModel? _selectedResult;
    private string _searchText;

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand AdvancedSearchButtonClickedCommand { get; }
    public ICommand AdvancedSearchCommand { get; }

    public VideoSearchResultViewModel? SelectedResult
    {
        get {
            _ = _selectedResult?.VideoViewer.LoadData();
            return _selectedResult;
        }
        set => SetProperty(ref _selectedResult, value);
    }

    public string SearchText
    {
        get => _searchText;
        set {
            _searchText = value;
            Task.Run(SearchAsync);
        }
    }

    public IEnumerable<Togglable<Genre>> Genres { get; }

    public VideoExplorerViewModel() : base(autoClose: false)
    {
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        AddCommand = new AsyncRelayCommand(Add);
        EditCommand = new AsyncRelayCommand(Edit);
        AdvancedSearchButtonClickedCommand = new RelayCommand(AdvancedSearchButtonClicked);
        AdvancedSearchCommand = new AsyncRelayCommand(AdvancedSearch);
        IsSearching = false;
        _searchText = "";
        _title = "";
        _actors = "";
        _roles = "";
        using var db = new DataContext();
        Genres = db.Genres.ToArray().Select(genre => new Togglable<Genre>(genre, isSelected: false)).ToArray();
    }

    private void AdvancedSearchButtonClicked()
    {
        Title = "";
        Actors = "";
        Roles = "";
        foreach (var item in Genres)
        {
            item.IsSelected = false;
        }
    }

    private async Task Add()
    {
        using var db = new DataContext();
        var video = new Video();
        db.Add(video);
        var editor = new VideoEditorViewModel(db, video, editionMode: false);
        if (await editor.Show())
        {
            await db.SaveChangesAsync();
            ImageChange.Apply();
            SelectedResult = new VideoSearchResultViewModel(VideoViewerViewModel.Create(video));
            SearchResults?.Add(SelectedResult);
        }
        else
        {
            ImageChange.Cancel();
        }
    }

    private async Task Edit()
    {
        if (SelectedResult is null) return;

        using var db = new DataContext();
        var video = SelectedResult.VideoViewer.Video.CopyFromDb(db);
        var editor = new VideoEditorViewModel(db, video, editionMode: true);
        if (await editor.Show())
        {
            if (editor.VideoWasRemoved)
            {
                db.Remove(video);
                await db.SaveChangesAsync();
                SearchResults?.Remove(SelectedResult);
                SelectedResult = null;
            }
            else
            {
                await db.SaveChangesAsync();
                ImageChange.Apply();

                var videoViewerViewModel = VideoViewerViewModel.Create(video);
                await videoViewerViewModel.LoadData();
                SelectedResult.VideoViewer = videoViewerViewModel;
            }
        }
        else
        {
            ImageChange.Cancel();
        }
    }

    private async Task SearchAsync()
    {
        IsSearching = true;

        using var db = new DataContext();
        SearchResults = new ObservableCollection<VideoSearchResultViewModel>(await Task.Run(() => db.Videos
            .Where(video => video.Title.ToLower().Contains((SearchText ?? "").ToLower()))
            .Select(video => new VideoSearchResultViewModel(VideoViewerViewModel.Create(video)))
            .ToArray()));
        IsSearching = false;
    }

    private async Task AdvancedSearch()
    {
        IsSearching = true;

        using var db = new DataContext();
        var selectedGenres = Genres.Where(g => g.IsSelected == true).Select(g => g.Value.Id).ToArray();
        var unselectedGenres = Genres.Where(g => g.IsSelected is null).Select(g => g.Value.Id).ToArray();

        SearchResults = new ObservableCollection<VideoSearchResultViewModel>(await db.Videos
            .Where(video => video.Title.ToLower().Contains((Title ?? "").ToLower())
            && Actors.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).All(actor => video.Roles.Any(role => role.Person.Name.ToLower().Contains(actor.ToLower())))
            && Roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).All(role => video.Roles.Any(r => !string.IsNullOrEmpty(r.Characters) && r.Characters.ToLower().Contains(role.ToLower())))
            && selectedGenres.All(g => video.Genres.Any(vg => vg.Id == g))
            && unselectedGenres.All(g => !video.Genres.Any(vg => vg.Id == g))
            //&& video.Roles.Any(role => Actors.ToLower().Contains(role.Person.Name.ToLower()))
            //&& video.Roles.Any(role => string.IsNullOrEmpty(role.Characters) && Roles.ToLower().Contains(role.Characters!.ToLower()))
            //&& Genres.Where(g => g.IsSelected == true).ToArray().All(g => video.Genres.Contains(g.Value))
            //&& Genres.Where(g => g.IsSelected == null).ToArray().All(g => !video.Genres.Contains(g.Value))
            )
            .Select(video => new VideoSearchResultViewModel(VideoViewerViewModel.Create(video)))
            .ToArrayAsync());

        IsSearching = false;
    }
}

public partial class VideoSearchResultViewModel : ObservableObject
{
    [ObservableProperty]
    private VideoViewerViewModel _videoViewer;

    public VideoSearchResultViewModel(VideoViewerViewModel videoViewerViewModel)
    {
        VideoViewer = videoViewerViewModel;
    }
}