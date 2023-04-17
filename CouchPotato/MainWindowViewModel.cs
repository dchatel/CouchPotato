using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using CouchPotato.Views;
using CouchPotato.Views.MigratorDialog;
using CouchPotato.Views.VideoExplorer;

namespace CouchPotato;

public class MainWindowViewModel
{
    public MainWindowViewModel()
    {
        OnLoadedCommand = new AsyncRelayCommand(OnLoaded);
    }

    public ObservableCollection<ContentViewModel> Pages { get; } = new();
    public ContentViewModel? CurrentPage { get; set; }
    public ICommand OnLoadedCommand { get; }

    public async Task OnLoaded()
    {
        var migrator = new MigratorViewModel();
        if (!await migrator.Show()) return;

        var videoExplorer = new VideoExplorerViewModel();
        _ = videoExplorer.Show();
    }
}
