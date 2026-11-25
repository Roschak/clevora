using System.Text.RegularExpressions;

namespace Clevora.Domain.Services;

/// <summary>
/// Converts WhatsApp markdown syntax to HTML for preview rendering.
/// Supported: *bold*, _italic_, ~strikethrough~, ```monospace```.
/// </summary>
public static class WhatsAppFormatter
{
    // Order matters: code block before inline patterns
    private static readonly (Regex Pattern, string Replacement)[] Rules =
    [
        (new Regex(@"```(.*?)```", RegexOptions.Singleline | RegexOptions.Compiled), "<code>$1</code>"),
        (new Regex(@"\*(.*?)\*", RegexOptions.Compiled), "<strong>$1</strong>"),
        (new Regex(@"_(.*?)_", RegexOptions.Compiled), "<em>$1</em>"),
        (new Regex(@"~(.*?)~", RegexOptions.Compiled), "<s>$1</s>"),
        (new Regex(@"\n", RegexOptions.Compiled), "<br/>"),
    ];

    /// <summary>Converts WhatsApp markdown in <paramref name="text"/> to HTML.</summary>
    public static string ToHtml(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        foreach (var (pattern, replacement) in Rules)
            text = pattern.Replace(text, replacement);
        return text;
    }

    /// <summary>Strips all WhatsApp markdown, returning plain text.</summary>
    public static string ToPlainText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        foreach (var (pattern, _) in Rules[..^1]) // skip newline rule
            text = pattern.Replace(text, "$1");
        return text;
    }
}
