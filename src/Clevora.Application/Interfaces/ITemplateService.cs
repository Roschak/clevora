using Clevora.Domain.Entities;

namespace Clevora.Application.Interfaces;

public interface ITemplateService
{
    Task<IReadOnlyList<MessageTemplate>> GetAllAsync(bool includeArchived = false, CancellationToken ct = default);
    Task<IReadOnlyList<MessageTemplate>> SearchAsync(string query, CancellationToken ct = default);
    Task<IReadOnlyList<MessageTemplate>> GetFavoritesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MessageTemplate>> GetByCategoryAsync(int categoryId, CancellationToken ct = default);
    Task<MessageTemplate?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MessageTemplate> CreateAsync(string title, string body, int categoryId, CancellationToken ct = default);
    Task<MessageTemplate> UpdateAsync(int id, string title, string body, int categoryId, CancellationToken ct = default);
    Task ToggleFavoriteAsync(int id, CancellationToken ct = default);
    Task ArchiveAsync(int id, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
