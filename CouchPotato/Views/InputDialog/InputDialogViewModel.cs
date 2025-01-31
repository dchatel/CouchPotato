using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

namespace CouchPotato.Views.InputDialog
{
    public partial class InputDialogViewModel : ContentViewModel
    {
        [ObservableProperty]
        private string? _inputText;

        public string HintMessage { get; }

        public InputDialogViewModel(string hintMessage) : base(autoClose: true)
        {
            HintMessage = hintMessage;
        }
    }
}
