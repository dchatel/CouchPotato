using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

namespace CouchPotato.Views;

public partial class PropertyUpdate<T> : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Selected))]
    private bool _update;

    public T Old { get; init; }
    public T New { get; init; }
    public T Selected => Update ? New : Old;

    public PropertyUpdate(T old, T @new, bool update = true)
    {
            Update = update;
        Old = old;
        New = @new;
    }
}
