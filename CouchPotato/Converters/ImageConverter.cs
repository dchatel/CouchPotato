using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace CouchPotato.Converters;

[MarkupExtensionReturnType(typeof(ImageConverter))]
public class ImageConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null) return null;
        if (!File.Exists((string)value)) return null;
        try
        {
            BitmapImage bmi = new();
            using (var stream = File.OpenRead((string)value))
            {
                bmi.BeginInit();
                bmi.CacheOption = BitmapCacheOption.OnLoad;
                bmi.StreamSource = stream;
                bmi.EndInit();
            }
            return bmi;
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}