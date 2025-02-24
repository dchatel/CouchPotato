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
    private bool _isSearching = false;
    [ObservableProperty]
    private ObservableCollection<VideoSearchResultViewModel>? _searchResults;
    [ObservableProperty]
    private string _title = "";
    [ObservableProperty]
    private string _actors = "";
    [ObservableProperty]
    private string _roles = "";
    private bool _dateActive = false;
    public bool DateActive
    {
        get => _dateActive;
        set {
            SetProperty(ref _dateActive, value);
            IsDateBefore = false;
            Year = null;
        }
    }
    [ObservableProperty]
    private bool _isDateBefore = false;
    [ObservableProperty]
    private int? _year = null;
    [ObservableProperty]
    private string _digitalStorageCode = "";
    [ObservableProperty]
    private string _digitalResolution = "";
    private bool _tmdbRatingActive = false;
    public bool TmdbRatingActive
    {
        get => _tmdbRatingActive;
        set {
            SetProperty(ref _tmdbRatingActive, value);
            IsTmdbRatingLesser = false;
            TmdbRating = null;
        }
    }
    [ObservableProperty]
    private bool _isTmdbRatingLesser = false;
    [ObservableProperty]
    private double? _tmdbRating = null;
    private bool _personalRatingActive = false;
    public bool PersonalRatingActive
    {
        get => _personalRatingActive;
        set {
            SetProperty(ref _personalRatingActive, value);
            IsPersonalRatingLesser = false;
            PersonalRating = null;
        }
    }
    [ObservableProperty]
    private bool _isPersonalRatingLesser = false;
    [ObservableProperty]
    private int? _personalRating = null;
    [ObservableProperty]
    private int? _runtime = null;

    private VideoSearchResultViewModel? _selectedResult;
    private string _searchText = "";

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
        using var db = new DataContext();
        Genres = db.Genres.ToArray().Select(genre => new Togglable<Genre>(genre, isSelected: false)).ToArray();
    }

    private void AdvancedSearchButtonClicked()
    {
        Title = "";
        Actors = "";
        Roles = "";
        DateActive = false;
        DigitalStorageCode = "";
        DigitalResolution = "";
        TmdbRatingActive = false;
        PersonalRatingActive = false;
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
            .Where(video => video.Title.Contains(SearchText))
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

        //SearchResults = new ObservableCollection<VideoSearchResultViewModel>(await db.Videos
        //    .Where(video => video.Title.ToLower().Contains((Title ?? "").ToLower())
        //    && Actors.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).All(actor => video.Roles.Any(role => role.Person.Name.ToLower().Contains(actor.ToLower())))
        //    && Roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).All(role => video.Roles.Any(r => !string.IsNullOrEmpty(r.Characters) && r.Characters.ToLower().Contains(role.ToLower())))
        //    && selectedGenres.All(g => video.Genres.Any(vg => vg.Id == g))
        //    && unselectedGenres.All(g => !video.Genres.Any(vg => vg.Id == g))
        //    )
        //    .Select(video => new VideoSearchResultViewModel(VideoViewerViewModel.Create(video)))
        //    .ToArrayAsync());

        var query = db.Videos.AsQueryable();
        if (!string.IsNullOrWhiteSpace(Title)) query = query.Where(video => EF.Functions.Collate(video.Title, "NO_ACCENTS") == EF.Functions.Collate(Title, "NO_ACCENTS"));
        if (!string.IsNullOrWhiteSpace(Actors)) query = query.Where(video => Actors.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).All(actor => video.Roles.Any(role => role.Person.Name.Contains(actor))));
        if (!string.IsNullOrWhiteSpace(Roles)) query = query.Where(video => Roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).All(role => video.Roles.Any(r => !string.IsNullOrEmpty(r.Characters) && r.Characters.Contains(role))));
        if (selectedGenres.Length != 0) query = query.Where(video => selectedGenres.All(g => video.Genres.Any(vg => vg.Id == g)));
        if (unselectedGenres.Length != 0) query = query.Where(video => unselectedGenres.All(g => !video.Genres.Any(vg => vg.Id == g)));
        if (DateActive && IsDateBefore && Year is not null) query = query.Where(video => video.ReleaseDate < new DateTime((int)Year, 1, 1, 0, 0, 0));
        if (DateActive && !IsDateBefore && Year is not null) query = query.Where(video => video.ReleaseDate > new DateTime((int)Year, 12, 31, 23, 59, 59));

        var results = await query
            .Select(video => new VideoSearchResultViewModel(VideoViewerViewModel.Create(video)))
            .ToArrayAsync();

        SearchResults = new ObservableCollection<VideoSearchResultViewModel>(results);

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