using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchPotato.Views
{
    public class Selectable
    {
        private bool isSelected;

        public object Value { get; }
        public bool IsSelected
        {
            get => isSelected;
            set => isSelected = value;
        }

        public Selectable(object value, bool isSelected = false)
        {
            Value = value;
            IsSelected = isSelected;
        }
    }
}
