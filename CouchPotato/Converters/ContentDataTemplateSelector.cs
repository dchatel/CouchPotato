using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace CouchPotato.Converters;

public class ContentDataTemplateSelector : DataTemplateSelector
{
    // 1. Make this logic reusable and static
    public static object? FindTemplate(Type type, string suffix)
    {
        // Try to find [ViewModelName][Suffix]
        var templateKey = type.Name.Replace("ViewModel", suffix);
        var template = App.Current.TryFindResource(templateKey);

        // Recursively check base types if not found
        if (template == null && type.BaseType is not null)
        {
            return FindTemplate(type.BaseType, suffix);
        }

        return template;
    }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is null) return null!;

        // Get the suffix from the control name (e.g., "Menu")
        var name = (string)container.GetValue(FrameworkElement.NameProperty);

        var template = FindTemplate(item.GetType(), name);

        // Return found template OR fallback to empty
        return (DataTemplate)(template ?? App.Current.TryFindResource("emptyDataTemplate"));
    }
}

[MarkupExtensionReturnType(typeof(HasContentTemplateConverter))]
public class HasContentTemplateConverter : MarkupExtension, IValueConverter
{
    [ConstructorArgument("suffix")]
    public string Suffix { get; set; }

    public HasContentTemplateConverter() : this(string.Empty) { }
    public HasContentTemplateConverter(string suffix)
    {
        Suffix = suffix;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
            return false;

        var template = ContentDataTemplateSelector.FindTemplate(value.GetType(), Suffix);

        return template is not null;
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