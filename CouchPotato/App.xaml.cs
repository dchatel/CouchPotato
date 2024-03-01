using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

using CouchPotato.Properties;

using Microsoft.Web.WebView2.Wpf;

namespace CouchPotato;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        var win = new MainWindow();
        win.ShowDialog();
        Config.Save();
    }

    public static void Restart()
    {
        Process.Start(Assembly.GetEntryAssembly()!.Location);
        Current.Shutdown();
    }
}