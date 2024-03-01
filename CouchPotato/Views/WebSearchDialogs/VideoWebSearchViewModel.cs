using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

using CouchPotato.DbModel;
using CouchPotato.DbModel.OtherDbModels.Tmdb;

using TMDbLib.Objects.General;

namespace CouchPotato.Views.WebSearchDialogs;

public class VideoWebSearchViewModel : ContentViewModel
{
    private readonly VideoType _videoType;
    private string? _title;
    private string? _year;
    private VideoSearchResult? _selectedVideo;

    public VideoWebSearchViewModel(VideoType videoType, string title, int? year) : base(autoClose: true)
    {
        SearchResults = Enumerable.Empty<VideoSearchResult>();
        SortedSearchResults = Enumerable.Empty<VideoSearchResult>();
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

    public IEnumerable<VideoSearchResult> SearchResults { get; set; }
    public IEnumerable<VideoSearchResult> SortedSearchResults { get; set; }
    public VideoSearchResult? SelectedVideo
    {
        get => _selectedVideo;
        set {
            _selectedVideo = value;
            if (_selectedVideo is not null)
                Url = $"https://www.themoviedb.org/{_selectedVideo.MediaType}/{_selectedVideo.TmdbId}";
        }
    }
    public string Url { get; set; }
    public bool Searching { get; set; }

    private async Task Search()
    {
        Searching = true;
        if (string.IsNullOrWhiteSpace(_title))
        {
            SearchResults = Enumerable.Empty<VideoSearchResult>();
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
