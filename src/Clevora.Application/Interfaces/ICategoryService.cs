using Clevora.Domain.Entities;

namespace Clevora.Application.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<TemplateCategory>> GetAllAsync(CancellationToken ct = default);
    Task<TemplateCategory?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TemplateCategory> CreateAsync(string name, string? description, string? icon, CancellationToken ct = default);
    Task<TemplateCategory> UpdateAsync(int id, string name, string? description, string? icon, CancellationToken ct = default);
    Task ArchiveAsync(int id, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
