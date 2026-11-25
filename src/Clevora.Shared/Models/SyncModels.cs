namespace Clevora.Shared.Models;

public enum SyncOperation
{
    Create = 0,
    Update = 1,
    Delete = 2
}

public record SyncPushItem(
    string EntityType,
    int EntityId,
    SyncOperation Operation,
    long BaseVersion,
    DateTime ClientUpdatedAtUtc,
    string PayloadJson);

public record SyncPushRequest(
    string DeviceId,
    List<SyncPushItem> Items);

public record SyncConflictItem(
    string EntityType,
    int EntityId,
    string ConflictReason,
    long ServerVersion,
    string? ServerPayloadJson);

public record SyncPushResponse(
    int AcceptedCount,
    List<SyncConflictItem> Conflicts);

public record SyncPullResponse(
    long LatestServerVersion,
    List<CategoryDto> Categories,
    List<TemplateDto> Templates,
    List<DraftDto> Drafts,
    List<int> DeletedCategoryIds,
    List<int> DeletedTemplateIds,
    List<int> DeletedDraftIds);
