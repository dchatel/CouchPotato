using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace CouchPotato.Converters;

[MarkupExtensionReturnType(typeof(LastItemConverter))]
//public class LastItemConverter : MarkupExtension, IValueConverter
//{
class LastItemConverter : MarkupExtension, IMultiValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[2] is not int count) return DependencyProperty.UnsetValue;

        if (values != null && values.Length == 3 && count > 0)
        {
            ItemsControl itemsControl = (ItemsControl)values[0];
            var itemContext = ((ContentPresenter)values[1]).DataContext;

            var lastItem = itemsControl.Items[count - 1];

            return Equals(lastItem, itemContext);
        }

        return DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
//    public override object ProvideValue(IServiceProvider serviceProvider)
//    {
//        return this;
//    }

//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        if (value is not DependencyObject item) return false;

//        ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(item);
//        return ic.ItemContainerGenerator.IndexFromContainer(item) == ic.Items.Count - 1;
//    }

//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        return Binding.DoNothing;
//    }
//}