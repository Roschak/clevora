using Clevora.Application.Interfaces;

namespace Clevora.Web.Api;

public static class TemplateEndpoints
{
    public record TemplateRequest(string Title, string Body, int CategoryId);

    public static void MapTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/templates")
            .WithTags("Templates")
            .RequireAuthorization();

        group.MapGet("/", async (ITemplateService svc, bool includeArchived = false, CancellationToken ct = default) =>
            Results.Ok(await svc.GetAllAsync(includeArchived, ct)))
            .WithSummary("Daftar semua template");

        group.MapGet("/favorites", async (ITemplateService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetFavoritesAsync(ct)))
            .WithSummary("Template favorit");

        group.MapGet("/search", async (string q, ITemplateService svc, CancellationToken ct) =>
            Results.Ok(await svc.SearchAsync(q, ct)))
            .WithSummary("Cari template");

        group.MapGet("/by-category/{categoryId:int}", async (int categoryId, ITemplateService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetByCategoryAsync(categoryId, ct)))
            .WithSummary("Template per kategori");

        group.MapGet("/{id:int}", async (int id, ITemplateService svc, CancellationToken ct) =>
        {
            var t = await svc.GetByIdAsync(id, ct);
            return t is null ? Results.NotFound() : Results.Ok(t);
        }).WithSummary("Detail template");

        group.MapPost("/", async (TemplateRequest req, ITemplateService svc, CancellationToken ct) =>
        {
            var t = await svc.CreateAsync(req.Title, req.Body, req.CategoryId, ct);
            return Results.Created($"/api/v1/templates/{t.Id}", t);
        }).WithSummary("Buat template baru");

        group.MapPut("/{id:int}", async (int id, TemplateRequest req, ITemplateService svc, CancellationToken ct) =>
        {
            try { return Results.Ok(await svc.UpdateAsync(id, req.Title, req.Body, req.CategoryId, ct)); }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        }).WithSummary("Update template");

        group.MapPost("/{id:int}/favorite", async (int id, ITemplateService svc, CancellationToken ct) =>
        {
            try { await svc.ToggleFavoriteAsync(id, ct); return Results.NoContent(); }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        }).WithSummary("Toggle favorit");

        group.MapPost("/{id:int}/archive", async (int id, ITemplateService svc, CancellationToken ct) =>
        {
            try { await svc.ArchiveAsync(id, ct); return Results.NoContent(); }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        }).WithSummary("Arsipkan template");
    }
}
