using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CouchPotato.Views;

public class PropertyUpdate<T> : INotifyPropertyChanged
{
    private bool _update;

    public bool Update
    {
        get => _update;
        set {
            _update = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Selected));
        }
    }
    public T Old { get; init; }
    public T New { get; init; }
    public T Selected => Update ? New : Old;

    public PropertyUpdate(T old, T @new, bool update = true)
    {
        Update = update;
        Old = old;
        New = @new;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
