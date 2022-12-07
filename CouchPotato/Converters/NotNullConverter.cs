using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace CouchPotato.Converters;

[MarkupExtensionReturnType(typeof(NotNullConverter))]
public class NotNullConverter : MarkupExtension, IValueConverter
{
    public NotNullConverter() : this(false) { }
    public NotNullConverter(bool reversed)
    {
        Reversed = reversed;
    }

    [ConstructorArgument(nameof(Reversed))]
    public bool Reversed { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new NotNullConverter(Reversed);
    }
}
