using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using PostSharp.Aspects.Advices;

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
        var template = name switch
        {
            "toolbar" => App.Current.TryFindResource(item.GetType().Name.Replace("ViewModel", "Toolbar")),
            "content" => App.Current.TryFindResource(item.GetType().Name.Replace("ViewModel", "View")),
            "menu" => App.Current.TryFindResource(item.GetType().Name.Replace("ViewModel", "Menu")),
            _ => null
        };

        return (DataTemplate)(template ?? App.Current.TryFindResource("emptyDataTemplate"));
    }
}
