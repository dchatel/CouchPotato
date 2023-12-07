using System;
using System.Windows;
using System.Windows.Controls;

namespace CouchPotato;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void RestoreButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}


public class ContentDataTemplateSelector : DataTemplateSelector
{
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is null) return null!;

        var name = (string)container.GetValue(FrameworkElement.NameProperty);

        static object? FindTemplate(Type type, string name)
        {
            var template = name switch
            {
                "toolbar" => App.Current.TryFindResource(type.Name.Replace("ViewModel", "Toolbar")),
                "content" => App.Current.TryFindResource(type.Name.Replace("ViewModel", "View")),
                "menu" => App.Current.TryFindResource(type.Name.Replace("ViewModel", "Menu")),
                _ => null
            };
            if (type.BaseType is not null)
                template ??= FindTemplate(type.BaseType, name);

            return template;
        }

        var template = FindTemplate(item.GetType(), name);

        return (DataTemplate)(template ?? App.Current.TryFindResource("emptyDataTemplate"));
    }
}
