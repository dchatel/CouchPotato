using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using CouchPotato.DbModel;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CouchPotato.Views.VideoEditor;

public class VideoEditorViewModel : ContentViewModel
{
    private readonly DataContext db;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public Video Video { get; }
    public string? PosterUrl { get; set; }
    public string? BackgroundUrl { get; set; }
    public IEnumerable<Selectable> Genres { get; private set; }

    public VideoEditorViewModel(int videoId) : base(autoClose: false)
    {
        db = new DataContext();

        SaveCommand = new AsyncRelayCommand(Save);
        CancelCommand = new RelayCommand(() => Close(false));

        Video = db.Videos
            .Include(v => v.Genres)
            .Single(v => v.Id == videoId);
        PosterUrl = Video.PosterUrl;
        BackgroundUrl = Video.BackgroundUrl;
        Genres = db.Genres
            .ToList()
            .Select(g => new Selectable(g, Video.Genres.Any(vg => vg.Id == g.Id)))
            .ToList();
    }

    public async Task Save()
    {
        foreach (var genre in Genres)
        {
            if (genre.IsSelected == true && Video.Genres.Contains(genre.Value) == false)
                Video.Genres.Add((Genre)genre.Value);
            else if (genre.IsSelected == false && Video.Genres.Contains(genre.Value) == true)
                Video.Genres.Remove((Genre)genre.Value);
        }
        await db.SaveChangesAsync();
        Close(true);
    }
}
