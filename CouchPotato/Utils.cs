using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using CouchPotato.Properties;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

using Path = System.IO.Path;

namespace CouchPotato;

public static class Utils
{
    public static string GetValidFileName(string fileName)
    {
        Debug.Assert(!string.IsNullOrEmpty(fileName));

        return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c, '_'));
    }

    public static void ResizeImage(string originalFilename, string resizedFilename, int? width = null, int? height = null)
    {
        using Image image = Image.Load(originalFilename);
        var options = new ResizeOptions
        {
            Size = new Size(width ?? image.Width, height ?? image.Height),
            Mode = ResizeMode.Max
        };

        image.Mutate(x => x.Resize(options));
        image.Save(resizedFilename);
    }
}

public class ImageChange
{
    private static readonly List<ImageChange> _imageChanges = new();

    public Image? Image { get; }
    public string? OldImage { get; }
    public string? NewImage { get; }

    public ImageChange(string? oldImage, string? newImage, string? newImagePath, int? width = null, int? height = null)
    {
        OldImage = oldImage;
        NewImage = newImage;

        if (newImage is not null && newImagePath is not null)
        {
            Image = Image.Load(newImagePath);
            if (width is not null || height is not null)
            {
                var options = new ResizeOptions
                {
                    Size = new Size(width ?? Image.Width, height ?? Image.Height),
                    Mode = ResizeMode.Max
                };

                Image.Mutate(x => x.Resize(options));
            }
            Image.Save(newImage);
        }
    }

    public static void Apply()
    {
        foreach (var change in _imageChanges)
        {
            if (change.OldImage is not null)
                File.Delete(change.OldImage);
        }
        _imageChanges.Clear();
    }

    public static void Cancel()
    {
        foreach (var change in _imageChanges)
        {
            if (change.NewImage is not null)
                File.Delete(change.NewImage);
        }
        _imageChanges.Clear();
    }

    public static string? AddImageChange(string? oldImage, string stem, string? suffix, int? width = null, int? height = null)
    {
        var newImagePath = GetFile();
        if (newImagePath is not null)
        {
            stem = Utils.GetValidFileName(stem);
            var newImage = $"Images/{stem}.{suffix}.{Guid.NewGuid()}.jpg";
            var change = new ImageChange(oldImage, newImage, newImagePath, width, height);
            _imageChanges.Add(change);
            return newImage;
        }
        else
        {
            return oldImage;
        }
    }

    private static string? GetFile()
    {
        var ofd = new System.Windows.Forms.OpenFileDialog
        {
            Title = Loc.SelectNewImage,
        };
        if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            return ofd.FileName;
        }
        else
        {
            return null;
        }
    }
}