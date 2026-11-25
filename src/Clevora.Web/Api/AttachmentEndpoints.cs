using Clevora.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clevora.Web.Api;

public static class AttachmentEndpoints
{
    public static void MapAttachmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/attachments")
            .WithTags("Attachments")
            .RequireAuthorization();

        group.MapPost("/upload", async (
            [FromForm] int templateId,
            IFormFile file,
            IAttachmentService attachmentService,
            CancellationToken ct) =>
        {
            if (file is null || file.Length == 0)
                return Results.BadRequest("File tidak boleh kosong.");

            // Maximum file size: 25MB
            if (file.Length > 25 * 1024 * 1024)
                return Results.BadRequest("Ukuran file maksimal 25 MB.");

            await using var stream = file.OpenReadStream();
            var result = await attachmentService.UploadAsync(
                templateId,
                stream,
                file.FileName,
                file.ContentType,
                ct);

            return Results.Created($"/api/v1/attachments/{result.Id}", result);
        })
        .DisableAntiforgery()
        .WithSummary("Unggah lampiran baru (PDF, Gambar, Brosur) ke template");

        group.MapGet("/template/{templateId:int}", async (
            int templateId,
            IAttachmentService attachmentService,
            CancellationToken ct) =>
        {
            var attachments = await attachmentService.GetByTemplateIdAsync(templateId, ct);
            return Results.Ok(attachments);
        })
        .WithSummary("Daftar lampiran untuk template tertentu");

        group.MapGet("/{id:int}/link", async (
            int id,
            [FromQuery] int? durationMinutes,
            IAttachmentService attachmentService,
            CancellationToken ct) =>
        {
            try
            {
                var ttl = durationMinutes.HasValue ? TimeSpan.FromMinutes(durationMinutes.Value) : (TimeSpan?)null;
                var link = await attachmentService.GenerateLinkAsync(id, ttl, ct);
                return Results.Ok(link);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound("Lampiran tidak ditemukan.");
            }
        })
        .WithSummary("Dapatkan tautan unduh aman (SAS) untuk disisipkan ke pesan WhatsApp");

        group.MapDelete("/{id:int}", async (
            int id,
            IAttachmentService attachmentService,
            CancellationToken ct) =>
        {
            try
            {
                await attachmentService.DeleteAsync(id, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithSummary("Hapus lampiran");
    }
}
