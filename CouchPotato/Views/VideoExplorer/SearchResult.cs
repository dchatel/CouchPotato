using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CouchPotato.DbModel;

namespace CouchPotato.Views.VideoExplorer;

public class SearchResult
{
    public SearchResult(Video video)
    {
        Video = video;
    }

    public Video Video { get; }
}
