using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

namespace CouchPotato
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            CheckForUpdates();

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        private async void CheckForUpdates()
        {
#if DEBUG
            return;
#endif
            using var client = new HttpClient();

            // Compare versions
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            if (currentVersion is null)
                return;

            var r = await client.GetAsync("https://github.com/dchatel/CouchPotato/releases/latest");
            var lastVersion = r.RequestMessage?.RequestUri?.Segments.Last();
            if (lastVersion is null)
                return;

            if (currentVersion.CompareTo(lastVersion) < 0)
            {
                // New version available
                using (var s = client.GetStreamAsync($"https://github.com/dchatel/CouchPotato/releases/download/{lastVersion}/CouchPotato.zip"))
                using (var fs = new FileStream($"{AppDomain.CurrentDomain.BaseDirectory}\\CouchPotato.zip", FileMode.OpenOrCreate))
                    s.Result.CopyTo(fs);
                App.Current.Shutdown();
                System.Diagnostics.Process.Start("powershell",
                    $"""
                    Expand-Archive -LiteralPath '{AppDomain.CurrentDomain.BaseDirectory}\\CouchPotato.zip' -DestinationPath '{AppDomain.CurrentDomain.BaseDirectory} -Force';
                    Read-Host
                    {AppDomain.CurrentDomain.BaseDirectory}\\CouchPotato.exe;
                    """);
            }
        }
    }
}
