using System;
using System.Globalization;
using System.IO;
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
        if (value is not string str) return null;
        if (str.StartsWith("http")) return HttpLoader(str);
        else if (str.StartsWith('/')) return TmdbLoader(str);
        else return FileSystemLoader(str);
    }

    private static BitmapImage? TmdbLoader(string str)
    {
        return HttpLoader($"https://www.themoviedb.org/t/p/w154{str}");
    }

    private static BitmapImage? HttpLoader(string url)
    {
        BitmapImage bmi = new();
        bmi.BeginInit();
        bmi.UriSource = new Uri(url, UriKind.Absolute);
        bmi.EndInit();
        return bmi;
    }

    private static BitmapImage? FileSystemLoader(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            BitmapImage bmi = new();
            using (var stream = File.OpenRead(path))
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