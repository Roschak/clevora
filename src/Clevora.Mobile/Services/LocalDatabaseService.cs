using System.Text.Json;
using Clevora.Domain.Entities;
using Clevora.Domain.Services;
using Clevora.Shared.Models;
using SQLite;

namespace Clevora.Mobile.Services;

// ── Outbox record for SQLite ──────────────────────────────────────────────────
[Table("SyncOutbox")]
public class SyncOutboxRecord
{
    [PrimaryKey] public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int Operation { get; set; } // SyncOperation enum value
    public long BaseVersion { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string PayloadJson { get; set; } = "{}";
}

// ── SQLite-mapped entity classes ──────────────────────────────────────────────
[Table("Categories")]
public class LocalCategory
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsArchived { get; set; }
    public bool IsDeleted { get; set; }
    public long Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("Templates")]
public class LocalTemplate
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsArchived { get; set; }
    public bool IsDeleted { get; set; }
    public string? Tags { get; set; }
    public long Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("AppState")]
public class AppStateRecord
{
    [PrimaryKey] public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

// ── Main service ──────────────────────────────────────────────────────────────
public class LocalDatabaseService
{
    private readonly SQLiteConnection _db;
    private const string LatestVersionKey = "LatestServerVersion";

    public LocalDatabaseService()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Clevora",
            "clevora_local.db");

        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        _db = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        _db.CreateTable<LocalCategory>();
        _db.CreateTable<LocalTemplate>();
        _db.CreateTable<SyncOutboxRecord>();
        _db.CreateTable<AppStateRecord>();

        SeedIfEmpty();
    }

    // ── Server version tracking ───────────────────────────────────────────────
    public long LatestServerVersion
    {
        get
        {
            var rec = _db.Find<AppStateRecord>(LatestVersionKey);
            return rec is not null && long.TryParse(rec.Value, out var v) ? v : 0;
        }
    }

    public void SetLatestServerVersion(long version)
    {
        _db.InsertOrReplace(new AppStateRecord { Key = LatestVersionKey, Value = version.ToString() });
    }

    // ── Categories ────────────────────────────────────────────────────────────
    public IReadOnlyList<LocalCategory> GetCategories() =>
        _db.Table<LocalCategory>()
           .Where(c => !c.IsDeleted && !c.IsArchived)
           .OrderBy(c => c.Name)
           .ToList();

    public LocalCategory? GetCategory(int id) =>
        _db.Find<LocalCategory>(id);

    public LocalCategory SaveCategory(string name, string? description, string? icon, int? id = null)
    {
        if (id.HasValue && id > 0)
        {
            var existing = _db.Find<LocalCategory>(id.Value)
                ?? throw new KeyNotFoundException("Category not found");
            existing.Name = name;
            existing.Description = description;
            existing.Icon = icon;
            existing.UpdatedAt = DateTime.UtcNow;
            _db.Update(existing);
            AddOutbox("category", existing.Id, SyncOperation.Update, existing.Version,
                JsonSerializer.Serialize(new UpdateCategoryRequest(name, description, icon, existing.Version)));
            return existing;
        }

        var cat = new LocalCategory { Name = name, Description = description, Icon = icon };
        _db.Insert(cat);
        AddOutbox("category", cat.Id, SyncOperation.Create, 0,
            JsonSerializer.Serialize(new CreateCategoryRequest(name, description, icon)));
        return cat;
    }

    public void DeleteCategory(int id)
    {
        var cat = _db.Find<LocalCategory>(id);
        if (cat is null) return;
        cat.IsDeleted = true;
        _db.Update(cat);
        AddOutbox("category", id, SyncOperation.Delete, cat.Version, "{}");
    }

    // ── Templates ─────────────────────────────────────────────────────────────
    public IReadOnlyList<LocalTemplate> GetTemplates(int? categoryId = null)
    {
        var query = _db.Table<LocalTemplate>().Where(t => !t.IsDeleted && !t.IsArchived);
        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);
        return query.OrderByDescending(t => t.IsFavorite).ThenByDescending(t => t.UpdatedAt).ToList();
    }

    public LocalTemplate? GetTemplate(int id) =>
        _db.Table<LocalTemplate>().FirstOrDefault(t => t.Id == id && !t.IsDeleted);

    public LocalTemplate SaveTemplate(string title, string body, int categoryId, string? tags, int? id = null)
    {
        if (id.HasValue && id > 0)
        {
            var existing = _db.Find<LocalTemplate>(id.Value)
                ?? throw new KeyNotFoundException("Template not found");
            existing.Title = title;
            existing.Body = body;
            existing.CategoryId = categoryId;
            existing.Tags = tags;
            existing.UpdatedAt = DateTime.UtcNow;
            _db.Update(existing);
            AddOutbox("template", existing.Id, SyncOperation.Update, existing.Version,
                JsonSerializer.Serialize(new UpdateTemplateRequest(title, body, categoryId, tags, existing.Version)));
            return existing;
        }

        var t = new LocalTemplate { Title = title, Body = body, CategoryId = categoryId, Tags = tags };
        _db.Insert(t);
        AddOutbox("template", t.Id, SyncOperation.Create, 0,
            JsonSerializer.Serialize(new CreateTemplateRequest(title, body, categoryId, tags)));
        return t;
    }

    public void DeleteTemplate(int id)
    {
        var t = _db.Find<LocalTemplate>(id);
        if (t is null) return;
        t.IsDeleted = true;
        _db.Update(t);
        AddOutbox("template", id, SyncOperation.Delete, t.Version, "{}");
    }

    public void ToggleFavorite(int id)
    {
        var t = _db.Find<LocalTemplate>(id);
        if (t is null) return;
        t.IsFavorite = !t.IsFavorite;
        t.UpdatedAt = DateTime.UtcNow;
        _db.Update(t);
        AddOutbox("template", id, SyncOperation.Update, t.Version,
            JsonSerializer.Serialize(new UpdateTemplateRequest(t.Title, t.Body, t.CategoryId, t.Tags, t.Version)));
    }

    // ── Outbox ────────────────────────────────────────────────────────────────
    public IReadOnlyList<SyncOutboxRecord> GetPendingOutbox() =>
        _db.Table<SyncOutboxRecord>().OrderBy(o => o.CreatedAtUtc).ToList();

    public void ClearOutbox(IEnumerable<string> ids)
    {
        var idList = ids.ToList();
        foreach (var id in idList)
            _db.Delete<SyncOutboxRecord>(id);
    }

    // ── Apply pull from server ────────────────────────────────────────────────
    public void ApplyRemotePull(SyncPullResponse pull)
    {
        SetLatestServerVersion(Math.Max(LatestServerVersion, pull.LatestServerVersion));

        // Soft-delete removed categories
        foreach (var cid in pull.DeletedCategoryIds)
        {
            var c = _db.Find<LocalCategory>(cid);
            if (c is null) continue;
            c.IsDeleted = true;
            _db.Update(c);
        }

        // Upsert categories
        foreach (var dto in pull.Categories)
        {
            var existing = _db.Find<LocalCategory>(dto.Id);
            if (existing is not null)
            {
                existing.Name = dto.Name;
                existing.Description = dto.Description;
                existing.Icon = dto.Icon;
                existing.IsArchived = dto.IsArchived;
                existing.Version = dto.Version;
                existing.UpdatedAt = dto.UpdatedAt;
                _db.Update(existing);
            }
            else
            {
                _db.InsertOrReplace(new LocalCategory
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Description = dto.Description,
                    Icon = dto.Icon,
                    IsArchived = dto.IsArchived,
                    Version = dto.Version,
                    CreatedAt = dto.CreatedAt,
                    UpdatedAt = dto.UpdatedAt
                });
            }
        }

        // Soft-delete removed templates
        foreach (var tid in pull.DeletedTemplateIds)
        {
            var t = _db.Find<LocalTemplate>(tid);
            if (t is null) continue;
            t.IsDeleted = true;
            _db.Update(t);
        }

        // Upsert templates
        foreach (var dto in pull.Templates)
        {
            var existing = _db.Find<LocalTemplate>(dto.Id);
            if (existing is not null)
            {
                existing.Title = dto.Title;
                existing.Body = dto.Body;
                existing.CategoryId = dto.CategoryId;
                existing.IsFavorite = dto.IsFavorite;
                existing.IsArchived = dto.IsArchived;
                existing.Tags = dto.Tags;
                existing.Version = dto.Version;
                existing.UpdatedAt = dto.UpdatedAt;
                _db.Update(existing);
            }
            else
            {
                _db.InsertOrReplace(new LocalTemplate
                {
                    Id = dto.Id,
                    Title = dto.Title,
                    Body = dto.Body,
                    CategoryId = dto.CategoryId,
                    IsFavorite = dto.IsFavorite,
                    IsArchived = dto.IsArchived,
                    Tags = dto.Tags,
                    Version = dto.Version,
                    CreatedAt = dto.CreatedAt,
                    UpdatedAt = dto.UpdatedAt
                });
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private void AddOutbox(string entityType, int entityId, SyncOperation op, long baseVersion, string payload)
    {
        _db.Insert(new SyncOutboxRecord
        {
            Id = Guid.NewGuid().ToString(),
            EntityType = entityType,
            EntityId = entityId,
            Operation = (int)op,
            BaseVersion = baseVersion,
            CreatedAtUtc = DateTime.UtcNow,
            PayloadJson = payload
        });
    }

    private void SeedIfEmpty()
    {
        if (_db.Table<LocalCategory>().Count() > 0) return;

        _db.Insert(new LocalCategory { Name = "Penjualan & Promo", Icon = "🏷️", Version = 1 });
        _db.Insert(new LocalCategory { Name = "Customer Support", Icon = "💬", Version = 1 });
        _db.Insert(new LocalCategory { Name = "Invoice & Tagihan", Icon = "💳", Version = 1 });

        _db.Insert(new LocalTemplate
        {
            CategoryId = 1,
            Title = "Konfirmasi Pesanan Baru",
            Body = "Halo *{{nama}}*, terima kasih telah memesan produk *{{produk}}*! Nomor resi Anda adalah `{{resi}}`.",
            IsFavorite = true,
            Version = 1
        });

        _db.Insert(new LocalTemplate
        {
            CategoryId = 3,
            Title = "Pengingat Pembayaran Tagihan",
            Body = "Yth. Bpk/Ibu *{{nama}}*, tagihan *{{layanan}}* sebesar ~Rp {{nominal}}~ jatuh tempo pada _{{tanggal}}_.",
            IsFavorite = true,
            Version = 1
        });
    }

    public void Dispose() => _db.Close();
}
