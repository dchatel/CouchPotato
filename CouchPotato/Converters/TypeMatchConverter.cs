using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace CouchPotato.Converters;

[MarkupExtensionReturnType(typeof(TypeMatchConverter))]
public class TypeMatchConverter : MarkupExtension, IValueConverter
{
    private readonly Type _type;

    public TypeMatchConverter(Type type)
    {
        _type = type;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value.GetType() == _type;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
