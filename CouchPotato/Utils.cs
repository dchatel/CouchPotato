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
