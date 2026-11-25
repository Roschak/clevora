using Clevora.Application.Interfaces;

namespace Clevora.Web.Api;

public static class ComposerEndpoints
{
    public record PreviewRequest(
        string TemplateBody,
        Dictionary<string, string> Values,
        string? RecipientPhone);

    public record AiRequest(AiOperation Operation, string Text, string? ExtraContext);

    public static void MapComposerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/composer")
            .WithTags("Composer")
            .RequireAuthorization();

        group.MapPost("/preview", (PreviewRequest req, IComposerService svc) =>
            Results.Ok(svc.Preview(req.TemplateBody, req.Values, req.RecipientPhone)))
            .WithSummary("Preview pesan dengan placeholder");

        group.MapPost("/ai", async (AiRequest req, IAiMessageService ai, CancellationToken ct) =>
        {
            var result = await ai.ProcessAsync(req.Operation, req.Text, req.ExtraContext, ct);
            return Results.Ok(new { Result = result });
        }).WithSummary("AI: Fix / Rewrite / Compose");
    }
}
