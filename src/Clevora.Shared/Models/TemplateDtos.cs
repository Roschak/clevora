namespace Clevora.Shared.Models;

public record CategoryDto(
    int Id,
    string Name,
    string? Description,
    string? Icon,
    bool IsArchived,
    long Version,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateCategoryRequest(
    string Name,
    string? Description,
    string? Icon);

public record UpdateCategoryRequest(
    string Name,
    string? Description,
    string? Icon,
    long Version);

public record TemplateDto(
    int Id,
    string Title,
    string Body,
    int CategoryId,
    string? CategoryName,
    bool IsFavorite,
    bool IsArchived,
    string? Tags,
    long Version,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<string> ExtractedPlaceholders);

public record CreateTemplateRequest(
    string Title,
    string Body,
    int CategoryId,
    string? Tags);

public record UpdateTemplateRequest(
    string Title,
    string Body,
    int CategoryId,
    string? Tags,
    long Version);

public record DraftDto(
    int Id,
    int? TemplateId,
    string Body,
    string? PlaceholderValues,
    string? RecipientPhone,
    long Version,
    DateTime UpdatedAt);

public record SaveDraftRequest(
    int? TemplateId,
    string Body,
    Dictionary<string, string> PlaceholderValues,
    string? RecipientPhone);
