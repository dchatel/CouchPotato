using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

namespace CouchPotato.Views;

public partial class Selectable<T> : ObservableObject
{
    private bool _isSelected;
    private readonly Action<T, bool>? _selectionChanged;

    public T Value { get; }
    public bool IsSelected
    {
        get => _isSelected;
        set {
            if (SetProperty(ref _isSelected, value))
            {
                OnPropertyChanged(nameof(IsSelected));
                _selectionChanged?.Invoke(Value, value);
            }
        }
    }

    public Selectable(T value, bool isSelected = false, Action<T, bool>? selectionChanged = null)
    {
        Value = value;
        IsSelected = isSelected;
        _selectionChanged = selectionChanged;
    }
}
