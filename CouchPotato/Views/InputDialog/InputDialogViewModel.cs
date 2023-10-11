using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchPotato.Views.InputDialog
{
    public class InputDialogViewModel : ContentViewModel
    {
        public string HintMessage { get; }
        public string? InputText { get; set; }

        public InputDialogViewModel(string hintMessage) : base(autoClose: true)
        {
            HintMessage = hintMessage;
        }
    }
}
