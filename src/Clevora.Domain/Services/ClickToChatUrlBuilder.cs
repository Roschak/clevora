using System.Text.RegularExpressions;

namespace Clevora.Domain.Services;

/// <summary>
/// Builds <c>https://wa.me/&lt;phone&gt;?text=&lt;encoded-message&gt;</c> URLs.
/// </summary>
public static class ClickToChatUrlBuilder
{
    private static readonly Regex NonDigitRegex =
        new(@"[^\d]", RegexOptions.Compiled);

    /// <summary>
    /// Builds a wa.me click-to-chat URL.
    /// </summary>
    /// <param name="phoneNumber">Phone number in any format; non-digits are stripped.</param>
    /// <param name="message">Pre-filled message text (optional).</param>
    /// <returns>A fully-encoded wa.me URL string.</returns>
    /// <exception cref="ArgumentException">Thrown when phone number is empty after stripping.</exception>
    public static string Build(string phoneNumber, string? message = null)
    {
        var digits = NonDigitRegex.Replace(phoneNumber ?? string.Empty, string.Empty);
        if (string.IsNullOrEmpty(digits))
            throw new ArgumentException("Nomor telepon tidak valid.", nameof(phoneNumber));

        var base64 = $"https://wa.me/{digits}";
        if (!string.IsNullOrEmpty(message))
            return $"{base64}?text={Uri.EscapeDataString(message)}";

        return base64;
    }

    /// <summary>Validates whether <paramref name="phoneNumber"/> yields a usable URL.</summary>
    public static bool IsValid(string? phoneNumber) =>
        !string.IsNullOrWhiteSpace(NonDigitRegex.Replace(phoneNumber ?? string.Empty, string.Empty));
}
