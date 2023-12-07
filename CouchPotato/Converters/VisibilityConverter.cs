using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace CouchPotato.Converters;

[MarkupExtensionReturnType(typeof(VisibilityConverter))]
public class VisibilityConverter : MarkupExtension, IValueConverter
{
    private readonly Visibility _falseVisibility = Visibility.Collapsed;
    private readonly Visibility _trueVisibility = Visibility.Visible;

    [ConstructorArgument(nameof(Reversed))]
    public bool Reversed { get; set; }

    public VisibilityConverter() : this(false) { }
    public VisibilityConverter(bool reversed)
    {
        if (reversed)
        {
            _falseVisibility = Visibility.Visible;
            _trueVisibility = Visibility.Collapsed;
        }
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return b ? _trueVisibility : _falseVisibility;
        if (value is string s)
        {
            if (parameter is string p)
                return s == p ? _trueVisibility : _falseVisibility;
            else
                return !string.IsNullOrEmpty(s) ? _trueVisibility : _falseVisibility;
        }
        return value is not null ? _trueVisibility : _falseVisibility;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new VisibilityConverter(Reversed);
    }
}
