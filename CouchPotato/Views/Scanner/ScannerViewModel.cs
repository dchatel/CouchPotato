using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using CouchPotato.DbModel;
using CouchPotato.Properties;

using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

using SixLabors.ImageSharp.Advanced;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CouchPotato.Views.Scanner;

public partial class ScannerViewModel : ContentViewModel
{
    private static readonly string[] _videoExtensions = { ".mkv", ".mp4", ".avi", ".mov", ".wmv" };

    [ObservableProperty]
    private string _path = string.Empty;
    [ObservableProperty]
    private string _digitalStorageCode = string.Empty;
    public ObservableCollection<ScannedItem> ScannedItems { get; } = [];

    public ScannerViewModel() : base(false)
    {
    }

    [RelayCommand]
    private void Cancel() => Close(false);

    [RelayCommand]
    private async Task Save()
    {
        using var db = new DataContext();
        foreach (var item in ScannedItems)
        {
            switch (item.Action)
            {
                case Action.Skip: break;
                case Action.Add:
                    var video = new Video
                    {
                        Title = item.SearchName,
                        DigitalStorageCode = DigitalStorageCode
                    };
                    db.Videos.Add(video);
                    break;
                case Action.Update:
                    if (item.Video is null) continue;
                    db.Attach(item.Video);
                    item.Video.DigitalStorageCode = DigitalStorageCode;
                    break;
            }
        }
        await db.SaveChangesAsync();
        Close(true);
    }

    [RelayCommand]
    private async Task SelectFolder()
    {
        var ofd = new OpenFolderDialog
        {
            Title = Loc.FileSystemSelectVidelibImageFolder,
            Multiselect = false,
        };
        Path = ofd.ShowDialog() == true ? ofd.FolderName : string.Empty;
        await Scan();
    }

    private async Task Scan()
    {
        ScannedItems.Clear();

        if (string.IsNullOrWhiteSpace(Path)) { return; }

        var enumerateOptions = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
        };
        var files = Directory.EnumerateFiles(Path, "*.*", enumerateOptions)
            .Where(f => _videoExtensions.Contains(System.IO.Path.GetExtension(f).ToLowerInvariant()))
            .ToList();

        using var db = new DataContext();
        var allVideos = await db.Videos.ToListAsync();
        var allVideoWords = allVideos
            .Select(v => new Tuple<string[], Video>(ScannedItem.Tokenize(v.Title), v))
            .ToList();

        foreach (var file in files)
        {
            var item = new ScannedItem(file, allVideoWords);

            ScannedItems.Add(item);
        }
    }
}
