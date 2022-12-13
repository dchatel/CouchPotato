using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CouchPotato.DbModel;

namespace CouchPotato.Views.VideoExplorer;

public class TVShowViewModel
{
    private TVShow tvshow;

    public TVShowViewModel(TVShow tvshow)
    {
        this.tvshow = tvshow;
    }

    public string Title => tvshow.Title;
}
