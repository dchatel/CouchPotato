using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace CouchPotato.Properties;

public partial class Config
{
    public string VidelibConnectionString { get; private set; } = @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadOnly;AttachDbFilename=C:\Users\dchat\Videlib.mdf";
    public string TMDbAPIKey { get; private set; } = "ae1e1d6d8fc9e53d1bb3f5501c690acd";
    public string Language { get; private set; } = "fr";

    public bool EnableAutoUpdates { get; set; } = false;

    public int MainWindowWidth { get; set; } = 800;
    public int MainWindowHeight { get; set; } = 600;
    public int MainWindowTop { get; set; } = 0;
    public int MainWindowLeft { get; set; } = 0;
    public WindowState MainWindowState { get; set; } = WindowState.Normal;
}
