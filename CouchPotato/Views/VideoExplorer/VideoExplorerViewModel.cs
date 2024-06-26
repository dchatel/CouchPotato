﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using CouchPotato.DbModel;
using CouchPotato.Views.VideoEditor;

namespace CouchPotato.Views.VideoExplorer;

public class VideoExplorerViewModel : ContentViewModel
{
    private VideoSearchResultViewModel? _selectedResult;
    private string _searchText;

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand SearchCommand { get; }
    public ObservableCollection<VideoSearchResultViewModel>? SearchResults { get; set; }
    public bool IsSearching { get; set; }

    public VideoSearchResultViewModel? SelectedResult
    {
        get => _selectedResult;
        set {
            _selectedResult = value;
            _selectedResult?.VideoViewer.LoadData();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set {
            _searchText = value;
            Task.Run(SearchAsync);
        }
    }

    public VideoExplorerViewModel() : base(autoClose: false)
    {
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        AddCommand = new AsyncRelayCommand(Add);
        EditCommand = new AsyncRelayCommand(Edit);
        IsSearching = false;
        _searchText = "";
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
                SelectedResult.VideoViewer.Video = video;
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

        SelectedResult = SearchResults.FirstOrDefault();
        IsSearching = false;
    }
}

public class VideoSearchResultViewModel
{
    public VideoViewerViewModel VideoViewer { get; set; }

    public VideoSearchResultViewModel(VideoViewerViewModel videoViewerViewModel)
    {
        VideoViewer = videoViewerViewModel;
    }
}