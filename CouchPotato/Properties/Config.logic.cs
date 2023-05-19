using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CouchPotato.Properties;

public partial class Config
{
    static Config()
    {
        Default = new Config();
        Load();
    }

    public static Config Default { get; private set; }

    public static void Load()
    {
        if (Path.Exists(ConfigPath))
        {
            var lines = File.ReadAllLines(ConfigPath, Encoding.UTF8);
            var re = KeyValueRegex();
            foreach (var line in lines)
            {
                var match = re.Match(line);
                Set(match.Groups["key"].Value, match.Groups["value"].Value);
            }
        }
    }

    public static void Save()
    {
        var props = typeof(Config).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var lines = props.Select(prop => $"{prop.Name}={prop.GetValue(Default)}").ToArray();
        File.WriteAllLines(ConfigPath, lines);
    }

    private static string ConfigPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CouchPotato.ini");

    [GeneratedRegex("^(?<key>\\w+)\\s*=\\s*(?<value>.*)$")]
    private static partial Regex KeyValueRegex();

    private static void Set(string key, object value)
    {
        var prop = typeof(Config).GetProperty(key, BindingFlags.Public | BindingFlags.Instance);
        if (prop is not null)
        {
            try
            {
                value = Convert.ChangeType(value, prop.PropertyType);
                prop.SetValue(Default, value);
            }
            catch
            {
                // Bad conversion, value ignored.
            }
        }
    }
}
