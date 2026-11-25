using Clevora.Application.Interfaces;
using Clevora.Shared.Models;

namespace Clevora.Web.Api;

public static class SyncEndpoints
{
    public static void MapSyncEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/sync")
            .WithTags("Mobile Sync")
            .RequireAuthorization();

        group.MapPost("/push", async (
            SyncPushRequest request,
            ISyncService syncService,
            CancellationToken ct) =>
        {
            var result = await syncService.PushAsync(request, ct);
            return Results.Ok(result);
        })
        .WithSummary("Push batch mutasi offline dari perangkat mobile");

        group.MapGet("/pull", async (
            long sinceVersion,
            string deviceId,
            ISyncService syncService,
            CancellationToken ct) =>
        {
            var result = await syncService.PullAsync(sinceVersion, deviceId, ct);
            return Results.Ok(result);
        })
        .WithSummary("Pull pembaruan delta dari server sejak versi terakhir");
    }
}
