using System.Text.Json;
using Clevora.Domain.Entities;
using Clevora.Shared.Models;

namespace Clevora.Mobile.Services;

public record SyncOutboxRecord(
    Guid Id,
    string EntityType,
    int EntityId,
    SyncOperation Operation,
    long BaseVersion,
    DateTime CreatedAtUtc,
    string PayloadJson);

public class LocalDatabaseService
{
    private readonly List<TemplateCategory> _categories = [];
    private readonly List<MessageTemplate> _templates = [];
    private readonly List<MessageDraft> _drafts = [];
    private readonly List<SyncOutboxRecord> _outbox = [];
    private long _latestServerVersion = 0;

    public LocalDatabaseService()
    {
        // Seed default starter templates if empty
        SeedStarterData();
    }

    public long LatestServerVersion => _latestServerVersion;
    public void SetLatestServerVersion(long version) => _latestServerVersion = version;

    public IReadOnlyList<TemplateCategory> GetCategories() =>
        _categories.Where(c => !c.IsDeleted && !c.IsArchived).OrderBy(c => c.Name).ToList();

    public IReadOnlyList<MessageTemplate> GetTemplates(int? categoryId = null) =>
        _templates.Where(t => !t.IsDeleted && !t.IsArchived && (!categoryId.HasValue || t.CategoryId == categoryId.Value))
                  .OrderByDescending(t => t.IsFavorite)
                  .ThenByDescending(t => t.UpdatedAt)
                  .ToList();

    public MessageTemplate? GetTemplate(int id) =>
        _templates.FirstOrDefault(t => t.Id == id && !t.IsDeleted);

    public MessageTemplate SaveTemplate(string title, string body, int categoryId, string? tags, int? id = null)
    {
        if (id.HasValue && id > 0)
        {
            var existing = _templates.FirstOrDefault(t => t.Id == id.Value)
                ?? throw new KeyNotFoundException("Template not found");
            existing.Title = title;
            existing.Body = body;
            existing.CategoryId = categoryId;
            existing.Tags = tags;
            existing.UpdatedAt = DateTime.UtcNow;

            _outbox.Add(new SyncOutboxRecord(
                Guid.NewGuid(), "template", existing.Id, SyncOperation.Update,
                existing.Version, DateTime.UtcNow,
                JsonSerializer.Serialize(new UpdateTemplateRequest(title, body, categoryId, tags, existing.Version))));

            return existing;
        }

        var newId = _templates.Any() ? _templates.Max(t => t.Id) + 1 : 1;
        var newT = new MessageTemplate
        {
            Id = newId,
            Title = title,
            Body = body,
            CategoryId = categoryId,
            Tags = tags,
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _templates.Add(newT);

        _outbox.Add(new SyncOutboxRecord(
            Guid.NewGuid(), "template", newId, SyncOperation.Create,
            0, DateTime.UtcNow,
            JsonSerializer.Serialize(new CreateTemplateRequest(title, body, categoryId, tags))));

        return newT;
    }

    public void DeleteTemplate(int id)
    {
        var existing = _templates.FirstOrDefault(t => t.Id == id);
        if (existing is not null)
        {
            existing.IsDeleted = true;
            _outbox.Add(new SyncOutboxRecord(
                Guid.NewGuid(), "template", id, SyncOperation.Delete,
                existing.Version, DateTime.UtcNow, "{}"));
        }
    }

    public void ToggleFavorite(int id)
    {
        var existing = _templates.FirstOrDefault(t => t.Id == id);
        if (existing is not null)
        {
            existing.IsFavorite = !existing.IsFavorite;
            existing.UpdatedAt = DateTime.UtcNow;
            _outbox.Add(new SyncOutboxRecord(
                Guid.NewGuid(), "template", id, SyncOperation.Update,
                existing.Version, DateTime.UtcNow,
                JsonSerializer.Serialize(new UpdateTemplateRequest(existing.Title, existing.Body, existing.CategoryId, existing.Tags, existing.Version))));
        }
    }

    public IReadOnlyList<SyncOutboxRecord> GetPendingOutbox() => _outbox.ToList();

    public void ClearOutbox(IEnumerable<Guid> ids)
    {
        var idSet = ids.ToHashSet();
        _outbox.RemoveAll(o => idSet.Contains(o.Id));
    }

    public void ApplyRemotePull(SyncPullResponse pull)
    {
        _latestServerVersion = Math.Max(_latestServerVersion, pull.LatestServerVersion);

        // Delete Categories
        var delCatSet = pull.DeletedCategoryIds.ToHashSet();
        _categories.RemoveAll(c => delCatSet.Contains(c.Id));

        // Upsert Categories
        foreach (var c in pull.Categories)
        {
            var match = _categories.FirstOrDefault(x => x.Id == c.Id);
            if (match is not null)
            {
                match.Name = c.Name;
                match.Description = c.Description;
                match.Icon = c.Icon;
                match.IsArchived = c.IsArchived;
                match.Version = c.Version;
                match.UpdatedAt = c.UpdatedAt;
            }
            else
            {
                _categories.Add(new TemplateCategory
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Icon = c.Icon,
                    IsArchived = c.IsArchived,
                    Version = c.Version,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                });
            }
        }

        // Delete Templates
        var delTmplSet = pull.DeletedTemplateIds.ToHashSet();
        _templates.RemoveAll(t => delTmplSet.Contains(t.Id));

        // Upsert Templates
        foreach (var t in pull.Templates)
        {
            var match = _templates.FirstOrDefault(x => x.Id == t.Id);
            if (match is not null)
            {
                match.Title = t.Title;
                match.Body = t.Body;
                match.CategoryId = t.CategoryId;
                match.IsFavorite = t.IsFavorite;
                match.IsArchived = t.IsArchived;
                match.Tags = t.Tags;
                match.Version = t.Version;
                match.UpdatedAt = t.UpdatedAt;
            }
            else
            {
                _templates.Add(new MessageTemplate
                {
                    Id = t.Id,
                    Title = t.Title,
                    Body = t.Body,
                    CategoryId = t.CategoryId,
                    IsFavorite = t.IsFavorite,
                    IsArchived = t.IsArchived,
                    Tags = t.Tags,
                    Version = t.Version,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                });
            }
        }
    }

    private void SeedStarterData()
    {
        _categories.Add(new TemplateCategory { Id = 1, Name = "Penjualan & Promo", Icon = "🏷️", Version = 1 });
        _categories.Add(new TemplateCategory { Id = 2, Name = "Customer Support", Icon = "💬", Version = 1 });
        _categories.Add(new TemplateCategory { Id = 3, Name = "Invoice & Tagihan", Icon = "💳", Version = 1 });

        _templates.Add(new MessageTemplate
        {
            Id = 1,
            CategoryId = 1,
            Title = "Konfirmasi Pesanan Baru",
            Body = "Halo *{{nama}}*, terima kasih telah memesan produk *{{produk}}*! Nomor resi Anda adalah `{{resi}}`.",
            IsFavorite = true,
            Version = 1
        });

        _templates.Add(new MessageTemplate
        {
            Id = 2,
            CategoryId = 3,
            Title = "Pengingat Pembayaran Tagihan",
            Body = "Yth. Bpk/Ibu *{{nama}}*, tagihan *{{layanan}}* sebesar ~Rp {{nominal}}~ jatuh tempo pada _{{tanggal}}_.",
            IsFavorite = true,
            Version = 1
        });
    }
}
