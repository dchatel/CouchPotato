using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using PostSharp.Patterns.Model;

namespace CouchPotato.Views;

public abstract class ContentViewModel : INotifyPropertyChanged
{
    readonly CancellationTokenSource cancellationTokenSource = new();
    bool dialogResult = false;

    [SafeForDependencyAnalysis]
    public bool IsCurrent => ((MainWindowViewModel)App.Current.MainWindow.DataContext).Pages.Last() == this;
    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    protected virtual void OnLoaded() { }

    public object? TitleBarRegion { get; protected init; } = null;
    public object? HamburgerMenu { get; protected init; } = null;

    protected ContentViewModel()
    {
        OkCommand = new RelayCommand(() => Close(true));
        CancelCommand = new RelayCommand(() => Close(false));
    }

    public async Task<bool> Show()
    {
        var mainWindowViewModel = (MainWindowViewModel)App.Current.MainWindow.DataContext;

        mainWindowViewModel.Pages.Add(this);
        mainWindowViewModel.CurrentPage = this;
        foreach (var page in mainWindowViewModel.Pages)
            page.OnPropertyChanged(nameof(IsCurrent));
        OnLoaded();
        try
        {
            await Task.Delay(-1, cancellationTokenSource.Token);
        }
        catch (TaskCanceledException) { }
        return dialogResult;
    }

    public void Close(bool result)
    {
        var mainWindowViewModel = (MainWindowViewModel)App.Current.MainWindow.DataContext;

        mainWindowViewModel.Pages.Remove(this);
        mainWindowViewModel.CurrentPage = mainWindowViewModel.Pages.LastOrDefault();
        foreach (var page in mainWindowViewModel.Pages)
            page.OnPropertyChanged(nameof(IsCurrent));
        dialogResult = result;
        cancellationTokenSource.Cancel();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        => PropertyChanged?.Invoke(this, new(propertyName));
}
