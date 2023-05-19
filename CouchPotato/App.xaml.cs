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

namespace CouchPotato
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //if (Config.Default.EnableAutoUpdates)
            //{
            //    await CheckForUpdates();
            //}

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

        //private async Task CheckForUpdates()
        //{
        //    var root = AppDomain.CurrentDomain.BaseDirectory;
        //    // Remove old files
        //    foreach (var oldFile in Directory.GetFiles(root, "*.old"))
        //        File.Delete(oldFile);

        //    using var client = new HttpClient();

        //    // Compare versions
        //    var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        //    if (currentVersion is null)
        //        return;

        //    var r = await client.GetAsync("https://github.com/dchatel/CouchPotato/releases/latest");
        //    var lastVersion = r.RequestMessage?.RequestUri?.Segments.Last();
        //    if (lastVersion is null)
        //        return;

        //    if (currentVersion.CompareTo(lastVersion) < 0)
        //    {
        //        // New version available
        //        using var s = client.GetStreamAsync($"https://github.com/dchatel/CouchPotato/releases/download/{lastVersion}/CouchPotato.zip");
        //        using var stream = new MemoryStream();
        //        s.Result.CopyTo(stream);

        //        var zip = new ZipArchive(stream);
        //        foreach (var entry in zip.Entries)
        //        {
        //            var maybeFile = Path.Combine(root, entry.Name);
        //            if (Path.Exists(maybeFile))
        //                File.Move(maybeFile, $"{maybeFile}.old");
        //            entry.ExtractToFile(maybeFile);
        //        }

        //        App.Current.Shutdown();
        //        MessageBox.Show(CouchPotato.Properties.Loc.UpdatedRestartPlease);
        //    }
        //}
    }
}
