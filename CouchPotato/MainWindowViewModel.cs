using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using CouchPotato.Views;
using CouchPotato.Views.MigratorDialog;
using CouchPotato.Views.VideoExplorer;

using PostSharp.Patterns.Model;

namespace CouchPotato;

public class MainWindowViewModel
{
    public MainWindowViewModel()
    {
        OnLoadedCommand = new AsyncRelayCommand(OnLoaded);
    }

    public ObservableCollection<ContentViewModel> Pages { get; } = new();
    public ContentViewModel? CurrentPage { get; set; }
    public ICommand OnLoadedCommand { get; init; }

    public async Task OnLoaded()
    {
        var migrator = new MigratorViewModel();
        if (!await migrator.Show()) return;

        var videoExplorer = new VideoExplorerViewModel();
        _ = videoExplorer.Show();
    }
}
