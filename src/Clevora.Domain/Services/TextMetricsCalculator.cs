namespace Clevora.Domain.Services;

/// <summary>Counts characters and words in a plain-text message body.</summary>
public static class TextMetricsCalculator
{
    /// <summary>Number of Unicode code-points (characters) in <paramref name="text"/>.</summary>
    public static int CharacterCount(string? text) =>
        string.IsNullOrEmpty(text) ? 0 : text.Length;

    /// <summary>
    /// Number of whitespace-delimited words in <paramref name="text"/>.
    /// Returns 0 for null/empty/whitespace-only strings.
    /// </summary>
    public static int WordCount(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>Estimated number of WhatsApp messages (1 message ≤ 4096 chars).</summary>
    public static int EstimatedMessageParts(string? text, int maxCharsPerPart = 4096)
    {
        int len = CharacterCount(text);
        return len == 0 ? 0 : (int)Math.Ceiling((double)len / maxCharsPerPart);
    }
}
