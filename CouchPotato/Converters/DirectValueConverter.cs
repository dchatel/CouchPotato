using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace CouchPotato.Converters;

public class DirectValueConverter : MarkupExtension, IValueConverter
{
    [ConstructorArgument(nameof(Value))]
    public object Value { get; set; }

    [ConstructorArgument(nameof(Reverse))]
    public bool Reverse { get; set; }

    public DirectValueConverter() : this(null!, false) { }
    public DirectValueConverter(object value, bool reverse)
    {
        Value = value;
        Reverse = reverse;
    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return (value.Equals(Value)) ^ Reverse;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return value is not null && ((bool)value ^ Reverse) ? Value : Binding.DoNothing;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}