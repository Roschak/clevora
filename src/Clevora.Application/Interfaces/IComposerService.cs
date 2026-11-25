namespace Clevora.Application.Interfaces;

public record ComposerPreviewResult(
    string RenderedBody,
    string HtmlPreview,
    int CharacterCount,
    int WordCount,
    string ClickToChatUrl,
    IReadOnlyList<string> MissingPlaceholders);

public interface IComposerService
{
    ComposerPreviewResult Preview(
        string templateBody,
        IReadOnlyDictionary<string, string> values,
        string? recipientPhone = null);
}
