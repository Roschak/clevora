using Clevora.Application.Interfaces;
using Clevora.Domain.Entities;
using Clevora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clevora.Infrastructure.Services;

public class TemplateService(ClevoraDbContext db, ICurrentUserService currentUser) : ITemplateService
{
    private IQueryable<MessageTemplate> OwnedTemplates =>
        db.Templates.Where(t => t.OwnerId == currentUser.UserId);

    public async Task<IReadOnlyList<MessageTemplate>> GetAllAsync(bool includeArchived = false, CancellationToken ct = default) =>
        await OwnedTemplates
            .Where(t => includeArchived || !t.IsArchived)
            .Include(t => t.Category)
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MessageTemplate>> SearchAsync(string query, CancellationToken ct = default) =>
        await OwnedTemplates
            .Where(t => !t.IsArchived && (t.Title.Contains(query) || t.Body.Contains(query)))
            .Include(t => t.Category)
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MessageTemplate>> GetFavoritesAsync(CancellationToken ct = default) =>
        await OwnedTemplates
            .Where(t => t.IsFavorite && !t.IsArchived)
            .Include(t => t.Category)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MessageTemplate>> GetByCategoryAsync(int categoryId, CancellationToken ct = default) =>
        await OwnedTemplates
            .Where(t => t.CategoryId == categoryId && !t.IsArchived)
            .Include(t => t.Category)
            .ToListAsync(ct);

    public async Task<MessageTemplate?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await OwnedTemplates
            .Include(t => t.Category)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<MessageTemplate> CreateAsync(string title, string body, int categoryId, CancellationToken ct = default)
    {
        var template = new MessageTemplate
        {
            Title = title,
            Body = body,
            CategoryId = categoryId,
            OwnerId = currentUser.UserId
        };
        db.Templates.Add(template);
        await db.SaveChangesAsync(ct);
        return template;
    }

    public async Task<MessageTemplate> UpdateAsync(int id, string title, string body, int categoryId, CancellationToken ct = default)
    {
        var template = await GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Template {id} not found.");
        template.Title = title;
        template.Body = body;
        template.CategoryId = categoryId;
        template.Version++;
        template.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return template;
    }

    public async Task ToggleFavoriteAsync(int id, CancellationToken ct = default)
    {
        var template = await GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Template {id} not found.");
        template.IsFavorite = !template.IsFavorite;
        template.Version++;
        template.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task ArchiveAsync(int id, CancellationToken ct = default)
    {
        var template = await GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Template {id} not found.");
        template.IsArchived = true;
        template.Version++;
        template.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var template = await GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Template {id} not found.");
        template.IsDeleted = true;
        template.Version++;
        template.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
