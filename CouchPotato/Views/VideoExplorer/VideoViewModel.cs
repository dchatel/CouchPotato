using CouchPotato.DbModel;

using PostSharp.Patterns.Model;

namespace CouchPotato.Views.VideoExplorer;

public class VideoViewModel
{
    private Video video;

    public VideoViewModel(Video video)
    {
        this.video = video;
    }

    public string Title => video.Title;
}