using System.Net.Http.Json;
using Clevora.Shared.Models;

namespace Clevora.Mobile.Services;

public class MobileSyncOrchestrator(
    LocalDatabaseService localDb,
    IHttpClientFactory httpClientFactory)
{
    private readonly string _deviceId = Guid.NewGuid().ToString("N");
    public bool IsSyncing { get; private set; }
    public DateTime? LastSyncTime { get; private set; }
    public string LastSyncStatus { get; private set; } = "Belum pernah disinkronkan";
    public List<SyncConflictItem> ActiveConflicts { get; private set; } = [];

    public async Task<bool> SynchronizeAsync(string baseUrl, string authToken)
    {
        if (IsSyncing) return false;
        IsSyncing = true;
        LastSyncStatus = "Sedang sinkronisasi...";

        try
        {
            var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Phase 1: Push
            var pending = localDb.GetPendingOutbox();
            if (pending.Any())
            {
                var pushItems = pending.Select(p => new SyncPushItem(
                    p.EntityType,
                    p.EntityId,
                    p.Operation,
                    p.BaseVersion,
                    p.CreatedAtUtc,
                    p.PayloadJson)).ToList();

                var pushReq = new SyncPushRequest(_deviceId, pushItems);
                var pushResp = await client.PostAsJsonAsync("/api/v1/sync/push", pushReq);

                if (pushResp.IsSuccessStatusCode)
                {
                    var pushResult = await pushResp.Content.ReadFromJsonAsync<SyncPushResponse>();
                    if (pushResult is not null)
                    {
                        ActiveConflicts = pushResult.Conflicts;
                        var successfulGuids = pending.Select(p => p.Id);
                        localDb.ClearOutbox(successfulGuids);
                    }
                }
            }

            // Phase 2: Pull
            var pullResp = await client.GetFromJsonAsync<SyncPullResponse>(
                $"/api/v1/sync/pull?sinceVersion={localDb.LatestServerVersion}&deviceId={_deviceId}");

            if (pullResp is not null)
            {
                localDb.ApplyRemotePull(pullResp);
            }

            LastSyncTime = DateTime.UtcNow;
            LastSyncStatus = $"Berhasil disinkronkan ({DateTime.Now:HH:mm})";
            return true;
        }
        catch (Exception ex)
        {
            LastSyncStatus = $"Gagal sinkron (Offline): {ex.Message}";
            return false;
        }
        finally
        {
            IsSyncing = false;
        }
    }
}
