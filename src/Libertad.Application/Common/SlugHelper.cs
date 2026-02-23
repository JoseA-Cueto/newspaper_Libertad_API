using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Libertad.Application.Common;

public static class SlugHelper
{
    public static string GenerateSlug(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim().ToLowerInvariant();
        var normalized = trimmed.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var ch in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }

        var noDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);
        var replaced = Regex.Replace(noDiacritics, "[^a-z0-9]+", "-");
        var collapsed = Regex.Replace(replaced, "-+", "-");
        return collapsed.Trim('-');
    }
}
