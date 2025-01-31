using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CouchPotato.Views.OkDialog
{
    public partial class OkDialogViewModel : ContentViewModel
    {
        [ObservableProperty]
        private object _content;

        public static ICommand ShowCommand { get; } = new AsyncRelayCommand<object>(Show);

        public OkDialogViewModel(object content) : base(autoClose: true)
        {
            Content = content;
        }

        public static async Task Show(object? content)
        {
            if (content is not null)
            {
                var okDialog = new OkDialogViewModel(content);
                await okDialog.Show();
            }
        }
    }
}
