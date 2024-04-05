using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

using GridControl = System.Windows.Controls.Grid;

namespace CouchPotato.Extensions;

public partial class Grid : DependencyObject
{
    public static string GetRows(DependencyObject obj) => (string)obj.GetValue(RowsProperty);
    public static void SetRows(DependencyObject obj, string value) => obj.SetValue(RowsProperty, value);
    public static readonly DependencyProperty RowsProperty =
        DependencyProperty.RegisterAttached("Rows", typeof(string), typeof(Grid), new PropertyMetadata(default(string), RowsChanged));

    public static string GetCols(DependencyObject obj) => (string)obj.GetValue(ColsProperty);
    public static void SetCols(DependencyObject obj, string value) => obj.SetValue(ColsProperty, value);
    public static readonly DependencyProperty ColsProperty =
        DependencyProperty.RegisterAttached("Cols", typeof(string), typeof(Grid), new PropertyMetadata(default(string), ColsChanged));

    private static void ColsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridControl grid && e.NewValue is string definitions)
        {
            var converter = new GridLengthConverter();
            grid.ColumnDefinitions.Clear();
            foreach (var def in definitions.Split(',').Select(x=>x.Trim()))
            {
                var match = GridRowColSizeList().Match(def);
                if (match.Success)
                {
                    var col = new ColumnDefinition
                    {
                        Width = (GridLength)converter.ConvertFromString(match.Groups["size"].Value)!
                    };
                    if (match.Groups.ContainsKey("minsize") && match.Groups["minsize"].Success)
                        col.MinWidth = double.Parse(match.Groups["minsize"].Value);
                    if (match.Groups.ContainsKey("maxsize") && match.Groups["maxsize"].Success)
                        col.MaxWidth = double.Parse(match.Groups["maxsize"].Value);

                    grid.ColumnDefinitions.Add(col);
                }
            }
        }
    }

    private static void RowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridControl grid && e.NewValue is string definitions)
        {
            var converter = new GridLengthConverter();
            grid.RowDefinitions.Clear();
            foreach (var def in definitions.Split(',').Select(x=>x.Trim()))
            {
                var match = GridRowColSizeList().Match(def);
                if (match.Success)
                {
                    var col = new RowDefinition
                    {
                        Height = (GridLength)converter.ConvertFromString(match.Groups["size"].Value)!
                    };
                    if (match.Groups.ContainsKey("minsize") && match.Groups["minsize"].Success)
                        col.MinHeight = double.Parse(match.Groups["minsize"].Value);
                    if (match.Groups.ContainsKey("maxsize") && match.Groups["maxsize"].Success)
                        col.MaxHeight = double.Parse(match.Groups["maxsize"].Value);

                    grid.RowDefinitions.Add(col);
                }
            }
        }
    }

    [GeneratedRegex("(?<size>auto|\\d*\\*?)\\s*(?:\\((?<minsize>\\d+)(?:-(?<maxsize>\\d+))\\))?")]
    private static partial Regex GridRowColSizeList();
}