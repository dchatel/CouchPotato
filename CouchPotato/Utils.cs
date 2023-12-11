using System.Diagnostics;
using System.Linq;

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
        //using var fs = new FileStream(originalFilename, FileMode.Open, FileAccess.Read);
        //using Image image = Image.FromStream(fs);
        //var ratioX = width is null ? 1.0 : (double)width / image.Width;
        //var ratioY = height is null ? 1.0 : (double)height / image.Height;
        //var ratio = Math.Min(ratioX, ratioY);

        //var newWidth = (int)(image.Width * ratio);
        //var newHeight = (int)(image.Height * ratio);

        //var newImage = new Bitmap(newWidth, newHeight);

        //using (Graphics graphics = Graphics.FromImage(newImage))
        //{
        //    graphics.DrawImage(image, 0, 0, newWidth, newHeight);
        //}

        //newImage.Save(resizedFilename, ImageFormat.Jpeg);
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
