namespace Clevora.Application.Interfaces;

public enum AiOperation { Fix, Rewrite, Compose }

public interface IAiMessageService
{
    /// <summary>
    /// Applies an AI operation on <paramref name="text"/>.
    /// Fix and Rewrite use gpt-5-mini; Compose uses gpt-5.6-luna.
    /// </summary>
    Task<string> ProcessAsync(
        AiOperation operation,
        string text,
        string? extraContext = null,
        CancellationToken ct = default);
}
