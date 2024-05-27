using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PostSharp.Patterns.Model;

namespace CouchPotato.Views;

[NotifyPropertyChanged]
public class Selectable<T>
{
    private bool _isSelected;
    private readonly Action<T, bool>? _selectionChanged;

    public T Value { get; }
    public bool IsSelected
    {
        get => _isSelected;
        set {
            _isSelected = value;
            _selectionChanged?.Invoke(Value, value);
        }
    }

    public Selectable(T value, bool isSelected = false, Action<T, bool>? selectionChanged = null)
    {
        Value = value;
        IsSelected = isSelected;
        _selectionChanged = selectionChanged;
    }
}
