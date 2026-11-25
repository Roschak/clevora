using Clevora.Shared.Models;

namespace Clevora.Application.Interfaces;

public interface ISyncService
{
    Task<SyncPushResponse> PushAsync(SyncPushRequest request, CancellationToken ct = default);
    Task<SyncPullResponse> PullAsync(long sinceVersion, string deviceId, CancellationToken ct = default);
}
