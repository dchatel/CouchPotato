using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;

using CouchPotato.DbModel;
using CouchPotato.DbModel.OtherDbModels.Tmdb;

using TMDbLib.Objects.General;

namespace CouchPotato.Views.WebSearchDialogs;

public partial class VideoWebSearchViewModel : ContentViewModel
{
    private readonly VideoType _videoType;
    private string? _title;
    private string? _year;
    private VideoSearchResult? _selectedVideo;

    public VideoWebSearchViewModel(VideoType videoType, string title, int? year) : base(autoClose: true)
    {
        SearchResults = [];
        SortedSearchResults = [];
        Url = "";
        _videoType = videoType;
        _title = title;
        _year = year.ToString();
        TitleSortAscending = true;
        YearSortAscending = null;
        Task.Run(Search);
    }

    public string? Title
    {
        get => _title;
        set {
            _title = value;
            Task.Run(Search);
        }
    }
    public string? Year
    {
        get => _year;
        set {
            _year = value;
            Task.Run(Search);
        }
    }

    public string? Characters { get; set; }
    public bool? TitleSortAscending { get; set; }
    public bool? YearSortAscending { get; set; }

    [ObservableProperty]
    public IEnumerable<VideoSearchResult> _searchResults;
    [ObservableProperty]
    public IEnumerable<VideoSearchResult> _sortedSearchResults;
    public VideoSearchResult? SelectedVideo
    {
        get => _selectedVideo;
        set {
            _selectedVideo = value;
            if (_selectedVideo is not null)
                Url = $"https://www.themoviedb.org/{_selectedVideo.MediaType}/{_selectedVideo.TmdbId}";
        }
    }

    [ObservableProperty]
    public string _url;
    [ObservableProperty]
    public bool _searching;

    private async Task Search()
    {
        Searching = true;
        if (string.IsNullOrWhiteSpace(_title))
        {
            SearchResults = [];
        }
        else
        {
            var mediaType = _videoType switch
            {
                VideoType.Movie => "movie",
                VideoType.TVShow => "tv",
                _ => "unknown",
            };
            if (int.TryParse(_year, out var year) == false)
                year = 0;
            var results = await Tmdb.SearchVideo(mediaType, Title ?? "", year);
            SearchResults = results;
            OnPropertyChanged(nameof(SortedSearchResults));
        }
        Searching = false;
    }
}
