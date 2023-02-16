using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CouchPotato.DbModel;

namespace CouchPotato.Views.VideoExplorer;

public class SearchResultViewModel
{
    public static SearchResultViewModel Create(Video video)
    {
        SearchResultViewModel result = video switch
        {
            Movie => new MovieSearchResultViewModel(video),
            TVShow => new TVShowSearchResultViewModel(video),
            _ => new SearchResultViewModel(video)
        };
        return result;
    }

    public SearchResultViewModel(Video video)
    {
        Video = video;
    }

    public bool IsSelected { get; set; }
    public Video Video { get; }
}

public class MovieSearchResultViewModel : SearchResultViewModel
{
    public MovieSearchResultViewModel(Video video) : base(video)
    {
    }
}

public class TVShowSearchResultViewModel : SearchResultViewModel
{
    public TVShowSearchResultViewModel(Video video) : base(video)
    {
    }
}