using System;
using System.Configuration;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

using Squirrel;

namespace CouchPotato
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            bool enableAutoUpdates = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableAutoUpdates"]);
            if (enableAutoUpdates)
            {
                using var mgr = new UpdateManager("https://github.com/dchatel/CouchPotato/releases");
                await mgr.UpdateApp();
            }

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }
    }
}
