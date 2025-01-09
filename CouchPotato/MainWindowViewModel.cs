using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using CouchPotato.Views;
using CouchPotato.Views.MigratorDialog;
using CouchPotato.Views.VideoExplorer;

using Microsoft.Extensions.DependencyModel;

namespace CouchPotato;

public partial class MainWindowViewModel:ObservableObject
{
    [ObservableProperty]
    private ContentViewModel? _currentPage;

    public MainWindowViewModel()
    {
        OnLoadedCommand = new AsyncRelayCommand(OnLoaded);
    }

    public ObservableCollection<ContentViewModel> Pages { get; } = [];
    public ICommand OnLoadedCommand { get; }

    public static async Task OnLoaded()
    {
        var migrator = new MigratorViewModel();
        if (!await migrator.Show()) return;

        var videoExplorer = new VideoExplorerViewModel();
        _ = videoExplorer.Show();
    }
}