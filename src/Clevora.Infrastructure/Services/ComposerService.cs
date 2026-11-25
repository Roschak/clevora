using Clevora.Application.Interfaces;
using Clevora.Domain.Services;

namespace Clevora.Infrastructure.Services;

public class ComposerService : IComposerService
{
    public ComposerPreviewResult Preview(
        string templateBody,
        IReadOnlyDictionary<string, string> values,
        string? recipientPhone = null)
    {
        var missing = PlaceholderParser.FindMissing(templateBody, values);
        var rendered = PlaceholderParser.Render(templateBody, values);
        var html = WhatsAppFormatter.ToHtml(rendered);
        var charCount = TextMetricsCalculator.CharacterCount(rendered);
        var wordCount = TextMetricsCalculator.WordCount(rendered);

        string clickUrl = string.Empty;
        if (!string.IsNullOrWhiteSpace(recipientPhone) && ClickToChatUrlBuilder.IsValid(recipientPhone))
            clickUrl = ClickToChatUrlBuilder.Build(recipientPhone, rendered);

        return new ComposerPreviewResult(rendered, html, charCount, wordCount, clickUrl, missing);
    }
}
