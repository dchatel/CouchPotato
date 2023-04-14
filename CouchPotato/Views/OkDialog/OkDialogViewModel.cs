using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace CouchPotato.Views.OkDialog
{
    public class OkDialogViewModel : ContentViewModel
    {
        public object Content { get; }

        public OkDialogViewModel(object content)
        {
            Content = content;
        }
    }
}
