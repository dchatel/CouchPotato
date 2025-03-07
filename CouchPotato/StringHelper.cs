using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using CouchPotato.Properties;

using Microsoft.EntityFrameworkCore;

namespace CouchPotato;

public partial class StringHelper
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

    [GeneratedRegex(@"^(\d+)\s*[xX]\s*(\d+)$")]
    private static partial Regex ParseResolutionRegex();
    public static int? GetResolution(string resolution)
    {
        var match = ParseResolutionRegex().Match(resolution);
        if (!match.Success) return null;
        var width = int.Parse(match.Groups[1].Value);
        var height = int.Parse(match.Groups[2].Value);
        return width * height;
    }

    public static int IsResolutionLesser(string a, string b)
    {
        var resolutionA = GetResolution(a);
        var resolutionB = GetResolution(b);
        return resolutionA is not null && resolutionA < resolutionB ? 0 : 1;
    }
    public static int IsResolutionEqual(string a, string b)
    {
        var resolutionA = GetResolution(a);
        var resolutionB = GetResolution(b);
        if (resolutionB is null) return resolutionA is null ? 0 : 1;
        return (resolutionB is null && resolutionA is null) || resolutionA == resolutionB ? 0 : 1;
    }
    public static int IsResolutionGreater(string a, string b)
    {
        var resolutionA = GetResolution(a);
        var resolutionB = GetResolution(b);
        return resolutionA is not null && resolutionA > resolutionB ? 0 : 1;
    }
}
