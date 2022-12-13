using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CouchPotato.DbModel;

namespace CouchPotato.Views.VideoExplorer;

public class MovieViewModel
{
    private Movie movie;

    public MovieViewModel(Movie movie)
    {
        this.movie = movie;
    }

    public string Title => movie.Title;
}
