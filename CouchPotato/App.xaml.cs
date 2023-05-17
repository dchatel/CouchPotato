using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

//using Squirrel;

namespace CouchPotato
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            MessageBox.Show($"Version: {Assembly.GetExecutingAssembly().GetName().Version}");
            //SquirrelAwareApp.HandleEvents(
            //    onInitialInstall: OnAppInstall,
            //    onAppUninstall: OnAppUninstall,
            //    onEveryRun: OnAppRun);

            //bool enableAutoUpdates = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableAutoUpdates"]);
            //if (enableAutoUpdates)
            //{
            //    using var mgr = new UpdateManager("https://github.com/dchatel/CouchPotato/releases");
            //    var newVersion = await mgr.UpdateApp();
            //    if(newVersion is not null)
            //    {
            //        UpdateManager.RestartApp();
            //    }
            //}

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        //private void OnAppInstall(SemanticVersion version, IAppTools tools)
        //{
        //    tools.CreateShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop);
        //}

        //private void OnAppUninstall(SemanticVersion version, IAppTools tools)
        //{
        //    tools.RemoveShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop);
        //}

        //private void OnAppRun(SemanticVersion version, IAppTools tools, bool firstRun)
        //{
        //    tools.SetProcessAppUserModelId();
        //}
    }
}
