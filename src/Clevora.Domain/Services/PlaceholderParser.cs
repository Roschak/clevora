using System.Text.RegularExpressions;

namespace Clevora.Domain.Services;

/// <summary>
/// Detects and resolves {{variable}} placeholders in a template body.
/// </summary>
public static class PlaceholderParser
{
    private static readonly Regex PlaceholderRegex =
        new(@"\{\{(\w+)\}\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>Returns all unique placeholder names found in <paramref name="body"/>.</summary>
    public static IReadOnlyList<string> ExtractNames(string body)
    {
        if (string.IsNullOrEmpty(body)) return Array.Empty<string>();
        return PlaceholderRegex.Matches(body)
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Replaces every {{key}} with its value from <paramref name="values"/>.
    /// Throws <see cref="InvalidOperationException"/> if any placeholder is missing or empty
    /// and <paramref name="requireAllFilled"/> is true.
    /// </summary>
    public static string Render(
        string body,
        IReadOnlyDictionary<string, string> values,
        bool requireAllFilled = false)
    {
        if (string.IsNullOrEmpty(body)) return body;

        var missing = new List<string>();

        string result = PlaceholderRegex.Replace(body, match =>
        {
            var key = match.Groups[1].Value;
            if (values.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val))
                return val;
            missing.Add(key);
            return match.Value; // keep original token
        });

        if (requireAllFilled && missing.Count > 0)
            throw new InvalidOperationException(
                $"Placeholder belum diisi: {string.Join(", ", missing)}");

        return result;
    }

    /// <summary>
    /// Returns placeholder names that are present in <paramref name="body"/>
    /// but missing or empty in <paramref name="values"/>.
    /// </summary>
    public static IReadOnlyList<string> FindMissing(
        string body,
        IReadOnlyDictionary<string, string> values)
    {
        return ExtractNames(body)
            .Where(n => !values.TryGetValue(n, out var v) || string.IsNullOrWhiteSpace(v))
            .ToList();
    }
}
