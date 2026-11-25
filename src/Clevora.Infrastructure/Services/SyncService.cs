using System.Text.Json;
using Clevora.Application.Interfaces;
using Clevora.Domain.Entities;
using Clevora.Infrastructure.Data;
using Clevora.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Clevora.Infrastructure.Services;

public class SyncService(ClevoraDbContext db, ICurrentUserService currentUser) : ISyncService
{
    public async Task<SyncPushResponse> PushAsync(SyncPushRequest request, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        int acceptedCount = 0;
        var conflicts = new List<SyncConflictItem>();

        foreach (var item in request.Items)
        {
            switch (item.EntityType.ToLowerInvariant())
            {
                case "template":
                    await ProcessTemplateMutationAsync(item, userId, conflicts, ct, () => acceptedCount++);
                    break;
                case "category":
                    await ProcessCategoryMutationAsync(item, userId, conflicts, ct, () => acceptedCount++);
                    break;
                case "draft":
                    await ProcessDraftMutationAsync(item, userId, conflicts, ct, () => acceptedCount++);
                    break;
            }
        }

        await db.SaveChangesAsync(ct);
        return new SyncPushResponse(acceptedCount, conflicts);
    }

    public async Task<SyncPullResponse> PullAsync(long sinceVersion, string deviceId, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;

        // Categories
        var categories = await db.Categories
            .IgnoreQueryFilters()
            .Where(c => c.OwnerId == userId && c.Version > sinceVersion)
            .ToListAsync(ct);

        var updatedCategories = categories.Where(c => !c.IsDeleted).Select(c => new CategoryDto(
            c.Id, c.Name, c.Description, c.Icon, c.IsArchived, c.Version, c.CreatedAt, c.UpdatedAt)).ToList();
        var deletedCategoryIds = categories.Where(c => c.IsDeleted).Select(c => c.Id).ToList();

        // Templates
        var templates = await db.Templates
            .IgnoreQueryFilters()
            .Include(t => t.Category)
            .Where(t => t.OwnerId == userId && t.Version > sinceVersion)
            .ToListAsync(ct);

        var updatedTemplates = templates.Where(t => !t.IsDeleted).Select(t => new TemplateDto(
            t.Id, t.Title, t.Body, t.CategoryId, t.Category?.Name, t.IsFavorite, t.IsArchived, t.Tags,
            t.Version, t.CreatedAt, t.UpdatedAt,
            Domain.Services.PlaceholderParser.ExtractNames(t.Body).ToList())).ToList();
        var deletedTemplateIds = templates.Where(t => t.IsDeleted).Select(t => t.Id).ToList();

        // Drafts
        var drafts = await db.Drafts
            .IgnoreQueryFilters()
            .Where(d => d.OwnerId == userId && d.Version > sinceVersion)
            .ToListAsync(ct);

        var updatedDrafts = drafts.Where(d => !d.IsDeleted).Select(d => new DraftDto(
            d.Id, d.TemplateId, d.Body, d.PlaceholderValues, d.RecipientPhone, d.Version, d.UpdatedAt)).ToList();
        var deletedDraftIds = drafts.Where(d => d.IsDeleted).Select(d => d.Id).ToList();

        long latestVersion = Math.Max(
            sinceVersion,
            Math.Max(
                categories.Any() ? categories.Max(c => c.Version) : 0,
                Math.Max(
                    templates.Any() ? templates.Max(t => t.Version) : 0,
                    drafts.Any() ? drafts.Max(d => d.Version) : 0)));

        return new SyncPullResponse(
            latestVersion,
            updatedCategories,
            updatedTemplates,
            updatedDrafts,
            deletedCategoryIds,
            deletedTemplateIds,
            deletedDraftIds);
    }

    private async Task ProcessTemplateMutationAsync(
        SyncPushItem item,
        string userId,
        List<SyncConflictItem> conflicts,
        CancellationToken ct,
        Action onAccepted)
    {
        var existing = await db.Templates
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == item.EntityId && t.OwnerId == userId, ct);

        if (item.Operation == SyncOperation.Create)
        {
            var req = JsonSerializer.Deserialize<CreateTemplateRequest>(item.PayloadJson);
            if (req is not null)
            {
                var newT = new MessageTemplate
                {
                    Title = req.Title,
                    Body = req.Body,
                    CategoryId = req.CategoryId,
                    Tags = req.Tags,
                    OwnerId = userId,
                    Version = 1,
                    CreatedAt = item.ClientUpdatedAtUtc,
                    UpdatedAt = item.ClientUpdatedAtUtc
                };
                db.Templates.Add(newT);
                onAccepted();
            }
            return;
        }

        if (existing is null)
        {
            conflicts.Add(new SyncConflictItem(item.EntityType, item.EntityId, "Item tidak ditemukan di server.", 0, null));
            return;
        }

        if (existing.Version > item.BaseVersion)
        {
            conflicts.Add(new SyncConflictItem(
                item.EntityType, item.EntityId,
                "Versi server lebih baru daripada versi dasar klien.",
                existing.Version,
                JsonSerializer.Serialize(existing)));
            return;
        }

        if (item.Operation == SyncOperation.Delete)
        {
            existing.IsDeleted = true;
            existing.Version++;
            existing.UpdatedAt = DateTime.UtcNow;
            onAccepted();
            return;
        }

        var updateReq = JsonSerializer.Deserialize<UpdateTemplateRequest>(item.PayloadJson);
        if (updateReq is not null)
        {
            existing.Title = updateReq.Title;
            existing.Body = updateReq.Body;
            existing.CategoryId = updateReq.CategoryId;
            existing.Tags = updateReq.Tags;
            existing.Version++;
            existing.UpdatedAt = DateTime.UtcNow;
            onAccepted();
        }
    }

    private async Task ProcessCategoryMutationAsync(
        SyncPushItem item,
        string userId,
        List<SyncConflictItem> conflicts,
        CancellationToken ct,
        Action onAccepted)
    {
        var existing = await db.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == item.EntityId && c.OwnerId == userId, ct);

        if (item.Operation == SyncOperation.Create)
        {
            var req = JsonSerializer.Deserialize<CreateCategoryRequest>(item.PayloadJson);
            if (req is not null)
            {
                var newCat = new TemplateCategory
                {
                    Name = req.Name,
                    Description = req.Description,
                    Icon = req.Icon,
                    OwnerId = userId,
                    Version = 1,
                    CreatedAt = item.ClientUpdatedAtUtc,
                    UpdatedAt = item.ClientUpdatedAtUtc
                };
                db.Categories.Add(newCat);
                onAccepted();
            }
            return;
        }

        if (existing is null)
        {
            conflicts.Add(new SyncConflictItem(item.EntityType, item.EntityId, "Kategori tidak ditemukan di server.", 0, null));
            return;
        }

        if (existing.Version > item.BaseVersion)
        {
            conflicts.Add(new SyncConflictItem(
                item.EntityType, item.EntityId,
                "Versi server lebih baru.",
                existing.Version,
                JsonSerializer.Serialize(existing)));
            return;
        }

        if (item.Operation == SyncOperation.Delete)
        {
            existing.IsDeleted = true;
            existing.Version++;
            existing.UpdatedAt = DateTime.UtcNow;
            onAccepted();
            return;
        }

        var updateReq = JsonSerializer.Deserialize<UpdateCategoryRequest>(item.PayloadJson);
        if (updateReq is not null)
        {
            existing.Name = updateReq.Name;
            existing.Description = updateReq.Description;
            existing.Icon = updateReq.Icon;
            existing.Version++;
            existing.UpdatedAt = DateTime.UtcNow;
            onAccepted();
        }
    }

    private async Task ProcessDraftMutationAsync(
        SyncPushItem item,
        string userId,
        List<SyncConflictItem> conflicts,
        CancellationToken ct,
        Action onAccepted)
    {
        var existing = await db.Drafts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == item.EntityId && d.OwnerId == userId, ct);

        if (item.Operation == SyncOperation.Create)
        {
            var req = JsonSerializer.Deserialize<SaveDraftRequest>(item.PayloadJson);
            if (req is not null)
            {
                var newD = new MessageDraft
                {
                    TemplateId = req.TemplateId,
                    Body = req.Body,
                    RecipientPhone = req.RecipientPhone,
                    PlaceholderValues = JsonSerializer.Serialize(req.PlaceholderValues),
                    OwnerId = userId,
                    Version = 1,
                    CreatedAt = item.ClientUpdatedAtUtc,
                    UpdatedAt = item.ClientUpdatedAtUtc
                };
                db.Drafts.Add(newD);
                onAccepted();
            }
            return;
        }

        if (existing is not null)
        {
            if (item.Operation == SyncOperation.Delete)
            {
                existing.IsDeleted = true;
                existing.Version++;
                existing.UpdatedAt = DateTime.UtcNow;
                onAccepted();
            }
            else
            {
                var req = JsonSerializer.Deserialize<SaveDraftRequest>(item.PayloadJson);
                if (req is not null)
                {
                    existing.Body = req.Body;
                    existing.RecipientPhone = req.RecipientPhone;
                    existing.PlaceholderValues = JsonSerializer.Serialize(req.PlaceholderValues);
                    existing.Version++;
                    existing.UpdatedAt = DateTime.UtcNow;
                    onAccepted();
                }
            }
        }
    }
}
