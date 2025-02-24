using System;
using System.Globalization;
using System.Text;

using Microsoft.EntityFrameworkCore;

namespace CouchPotato;

public class StringHelper
{
    public static int AccentInsensitiveComparison(string a, string b)
    {
        // Handle nulls appropriately
        if (a == null && b == null) return 0;
        if (a == null) return -1;
        if (b == null) return 1;

        var normalizedA = RemoveDiacritics(a).ToLowerInvariant();
        var normalizedB = RemoveDiacritics(b).ToLowerInvariant();
        return normalizedA.Contains(normalizedB) ? 0 : string.Compare(normalizedA, normalizedB, StringComparison.Ordinal);
    }

    public static string RemoveDiacritics(string text)
    {
        if (text == null) return "";
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
