using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace CouchPotato.Converters;

[MarkupExtensionReturnType(typeof(VisibilityConverter))]
public class VisibilityConverter : MarkupExtension, IValueConverter
{
    private readonly Visibility FalseVisibility = Visibility.Collapsed;
    private readonly Visibility TrueVisibility = Visibility.Visible;

    [ConstructorArgument(nameof(Reversed))]
    public bool Reversed { get; set; }

    public VisibilityConverter() : this(false) { }
    public VisibilityConverter(bool reversed)
    {
        if (reversed)
        {
            FalseVisibility = Visibility.Visible;
            TrueVisibility = Visibility.Collapsed;
        }
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return b ? TrueVisibility : FalseVisibility;
        if (value is string s)
        {
            if (parameter is string p)
                return s == p ? TrueVisibility : FalseVisibility;
            else
                return !string.IsNullOrEmpty(s) ? TrueVisibility : FalseVisibility;
        }
        return value is not null ? TrueVisibility : FalseVisibility;
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
