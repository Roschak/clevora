using Clevora.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Clevora.Web.Api;

public static class CategoryEndpoints
{
    public record CategoryRequest(string Name, string? Description, string? Icon);

    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/categories")
            .WithTags("Categories")
            .RequireAuthorization();

        group.MapGet("/", async (ICategoryService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetAllAsync(ct)))
            .WithSummary("Daftar semua kategori");

        group.MapGet("/{id:int}", async (int id, ICategoryService svc, CancellationToken ct) =>
        {
            var category = await svc.GetByIdAsync(id, ct);
            return category is null ? Results.NotFound() : Results.Ok(category);
        }).WithSummary("Detail kategori");

        group.MapPost("/", async (CategoryRequest req, ICategoryService svc, CancellationToken ct) =>
        {
            var cat = await svc.CreateAsync(req.Name, req.Description, req.Icon, ct);
            return Results.Created($"/api/v1/categories/{cat.Id}", cat);
        }).WithSummary("Buat kategori baru");

        group.MapPut("/{id:int}", async (int id, CategoryRequest req, ICategoryService svc, CancellationToken ct) =>
        {
            try
            {
                var cat = await svc.UpdateAsync(id, req.Name, req.Description, req.Icon, ct);
                return Results.Ok(cat);
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        }).WithSummary("Update kategori");

        group.MapPost("/{id:int}/archive", async (int id, ICategoryService svc, CancellationToken ct) =>
        {
            try { await svc.ArchiveAsync(id, ct); return Results.NoContent(); }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        }).WithSummary("Arsipkan kategori");
    }
}
