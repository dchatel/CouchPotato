using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchPotato;

public static class Utils
{
    public static string GetValidFileName(string fileName)
    {
        Debug.Assert(!string.IsNullOrEmpty(fileName));

        return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c, '_'));
    }
}
