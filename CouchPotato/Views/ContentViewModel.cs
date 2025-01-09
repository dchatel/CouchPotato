using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CouchPotato.Views;

public abstract partial class ContentViewModel : ObservableObject
{
    readonly CancellationTokenSource _cancellationTokenSource = new();
    bool _dialogResult = false;

    public bool IsCurrent => ((MainWindowViewModel)App.Current.MainWindow.DataContext).Pages.Last() == this;
    public ICommand OkCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand AutoCloseCommand { get; }

    protected virtual void OnLoaded() { }

    [ObservableProperty]
    bool _canAutoClose;
    [ObservableProperty]
    object? _hamburgerMenu;

    protected ContentViewModel(bool autoClose)
    {
        CanAutoClose = autoClose;
        OkCommand = new RelayCommand(() => Close(true));
        CloseCommand = new RelayCommand(() => Close(false));
        AutoCloseCommand = new RelayCommand(() => Close(false, true));
    }

    public async Task<bool> Show()
    {
        var mainWindowViewModel = (MainWindowViewModel)App.Current.MainWindow.DataContext;

        mainWindowViewModel.Pages.Add(this);
        mainWindowViewModel.CurrentPage?.OnPropertyChanged(nameof(IsCurrent));
        mainWindowViewModel.CurrentPage = this;
        mainWindowViewModel.CurrentPage.OnPropertyChanged(nameof(IsCurrent));
        OnLoaded();
        try
        {
            await Task.Delay(-1, _cancellationTokenSource.Token);
        }
        catch (TaskCanceledException) { }
        return _dialogResult;
    }

    public void Close(bool result, bool autoClose = false)
    {
        if (autoClose && !CanAutoClose) return;
        if (App.Current.MainWindow is null) return;

        var mainWindowViewModel = (MainWindowViewModel)App.Current.MainWindow.DataContext;

        mainWindowViewModel.Pages.Remove(this);
        mainWindowViewModel.CurrentPage = mainWindowViewModel.Pages.LastOrDefault();
        mainWindowViewModel.CurrentPage?.OnPropertyChanged(nameof(IsCurrent));
        _dialogResult = result;
        _cancellationTokenSource.Cancel();
    }
}
