using Clevora.Application.Interfaces;
using Clevora.Domain.Entities;
using Clevora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clevora.Infrastructure.Services;

public class CategoryService(ClevoraDbContext db, ICurrentUserService currentUser) : ICategoryService
{
    public async Task<IReadOnlyList<TemplateCategory>> GetAllAsync(CancellationToken ct = default) =>
        await db.Categories
            .Where(c => c.OwnerId == currentUser.UserId && !c.IsArchived)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<TemplateCategory?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == currentUser.UserId, ct);

    public async Task<TemplateCategory> CreateAsync(string name, string? description, string? icon, CancellationToken ct = default)
    {
        var category = new TemplateCategory
        {
            Name = name,
            Description = description,
            Icon = icon,
            OwnerId = currentUser.UserId
        };
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
        return category;
    }

    public async Task<TemplateCategory> UpdateAsync(int id, string name, string? description, string? icon, CancellationToken ct = default)
    {
        var category = await GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Category {id} not found.");
        category.Name = name;
        category.Description = description;
        category.Icon = icon;
        category.Version++;
        category.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return category;
    }

    public async Task ArchiveAsync(int id, CancellationToken ct = default)
    {
        var category = await GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Category {id} not found.");
        category.IsArchived = true;
        category.Version++;
        category.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Category {id} not found.");
        category.IsDeleted = true;
        category.Version++;
        category.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
