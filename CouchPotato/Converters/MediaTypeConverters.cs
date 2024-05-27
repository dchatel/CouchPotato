using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

using CouchPotato.DbModel;

using MaterialDesignThemes.Wpf;

namespace CouchPotato.Converters;

[MarkupExtensionReturnType(typeof(PackIconKind))]
public class MediaTypeToIconConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var iconKind = value switch
        {
            "movie" => PackIconKind.Movie,
            "tv" => PackIconKind.TvBox,
            _ => PackIconKind.QuestionMarkCircleOutline,
        };
        return iconKind;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}

[MarkupExtensionReturnType(typeof(Brush))]
public class MediaTypeToColorConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Brush color = value switch
        {
            "movie" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#55f")),
            "tv" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f55")),
            _ => Brushes.White,
        };
        return color;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}

[MarkupExtensionReturnType(typeof(MediaTypeToVisibilityConverter))]
public class MediaTypeToVisibilityConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Visibility visibility;
        if(value is VideoType videoType && videoType == VideoType.Unknown)
        {
            visibility = Visibility.Collapsed;
        }
        else
        {
            visibility = Visibility.Visible;
        }
        return visibility;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}