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
            SquirrelAwareApp.HandleEvents(
                onEveryRun: OnAppRun);

            bool enableAutoUpdates = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableAutoUpdates"]);
            if (enableAutoUpdates)
            {
                using var mgr = new UpdateManager("https://github.com/dchatel/CouchPotato/releases");
                var newVersion = await mgr.UpdateApp();
                if(newVersion is not null)
                {
                    UpdateManager.RestartApp();
                }
            }

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        private void OnAppRun(SemanticVersion version, IAppTools tools, bool firstRun)
        {
            tools.SetProcessAppUserModelId();
        }
    }
}
