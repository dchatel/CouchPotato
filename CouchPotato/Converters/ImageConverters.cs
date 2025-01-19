using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CouchPotato.Converters;

public abstract class ImageLoaderBase : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    private static BitmapImage LoadBitmapImage(string url)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.DownloadCompleted += (s, e) =>
        {
            if (bitmap.CanFreeze)
                bitmap.Freeze(); // Important for performance
        };
        bitmap.EndInit();
        return bitmap;
    }

    private static BitmapImage LoadFromTmdb(string path)
    {
        return LoadBitmapImage($"https://www.themoviedb.org/t/p/w154{path}");
    }

    protected static BitmapImage? GetImage(string str)
    {
        try
        {
            if (str.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return LoadBitmapImage(str);
            else if (str.StartsWith('/'))
                return LoadFromTmdb(str);
            else
                return LoadBitmapImage(str);
        }
        catch
        {
            return null;
        }
    }

    public abstract object? Convert(object value, Type targetType, object parameter, CultureInfo culture);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}

[MarkupExtensionReturnType(typeof(ImageConverter))]
public class ImageConverter : ImageLoaderBase
{
    public override object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string str)
            return null;
        return GetImage(str);
    }
}

[MarkupExtensionReturnType(typeof(IValueConverter))]
public class ImageBrushConverter : ImageLoaderBase
{
    private readonly Brush _fallbackBrush;

    public ImageBrushConverter() : this(Brushes.Transparent) { }
    public ImageBrushConverter(Brush fallbackBrush)
    {
        _fallbackBrush = fallbackBrush;
    }

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string str)
            return _fallbackBrush;

        BitmapImage? bitmap = GetImage(str);

        if (bitmap is null)
            return _fallbackBrush;

        return new ImageBrush(bitmap);
    }
}