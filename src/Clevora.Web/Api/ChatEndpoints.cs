using Clevora.Application.Interfaces;
using Clevora.Domain.Entities;
using Clevora.Infrastructure.Data;
using Clevora.Infrastructure.Identity;
using Clevora.Shared.Models;
using Clevora.Web.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Clevora.Web.Api;

public static class ChatEndpoints
{
    public static void MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/chat").RequireAuthorization("ApiPolicy").WithTags("Chat");

        // ── List my conversations ────────────────────────────────────────────
        group.MapGet("/conversations", async (
            IChatService chat,
            CancellationToken ct) =>
        {
            var summaries = await chat.GetMyConversationsAsync(ct);
            return Results.Ok(summaries);
        })
        .WithSummary("Dapatkan daftar percakapan saya");

        // ── Start private 1-on-1 chat ────────────────────────────────────────
        group.MapPost("/conversations/private", async (
            StartPrivateChatRequest req,
            IChatService chat,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.TargetUserId))
                return Results.BadRequest("TargetUserId wajib diisi.");

            var conversation = await chat.StartPrivateChatAsync(req.TargetUserId, ct);
            return Results.Ok(new ConversationDto(
                conversation.Id,
                conversation.Name ?? string.Empty,
                conversation.IsGroup,
                conversation.AvatarUrl,
                [],
                0));
        })
        .WithSummary("Mulai atau lanjutkan chat 1-on-1");

        // ── Create group ─────────────────────────────────────────────────────
        group.MapPost("/conversations/group", async (
            CreateGroupRequest req,
            IChatService chat,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest("Nama grup wajib diisi.");

            var conversation = await chat.CreateGroupAsync(req.Name, req.MemberIds, ct);
            return Results.Created($"/api/v1/chat/conversations/{conversation.Id}/messages",
                new ConversationDto(
                    conversation.Id,
                    conversation.Name ?? string.Empty,
                    conversation.IsGroup,
                    conversation.AvatarUrl,
                    [],
                    0));
        })
        .WithSummary("Buat grup chat baru");

        // ── Get messages (paged) ─────────────────────────────────────────────
        group.MapGet("/conversations/{id:int}/messages", async (
            int id,
            int? pageSize,
            long? beforeId,
            IChatService chat,
            UserManager<ClevoraUser> userManager,
            ClevoraDbContext db,
            CancellationToken ct) =>
        {
            var size = Math.Clamp(pageSize ?? 30, 1, 100);
            var messages = await chat.GetMessagesAsync(id, size, beforeId, ct);

            var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
            var users = await db.Users
                .Where(u => senderIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, ct);

            var dtos = messages.Select(m =>
            {
                users.TryGetValue(m.SenderId, out var sender);
                return new MessageDto(
                    m.Id,
                    m.ConversationId,
                    m.SenderId,
                    sender?.DisplayName ?? sender?.Email ?? "Unknown",
                    sender?.AvatarUrl,
                    m.Body,
                    m.Type,
                    m.Status,
                    m.ReplyToId,
                    m.IsDeleted,
                    m.SentAt,
                    false); // IsMine determined client-side by comparing SenderId
            }).ToList();

            return Results.Ok(dtos);
        })
        .WithSummary("Ambil pesan percakapan (paging)");

        // ── Send message via REST API ─────────────────────────────────────────
        group.MapPost("/conversations/{id:int}/messages", async (
            int id,
            SendMessageRequest req,
            IChatService chat,
            IHubContext<ChatHub> hubContext,
            UserManager<ClevoraUser> userManager,
            ICurrentUserService currentUser,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Body))
                return Results.BadRequest("Pesan tidak boleh kosong.");

            var message = await chat.SendMessageAsync(id, req.Body.Trim(), MessageType.Text, req.ReplyToId, ct);
            var sender = await userManager.FindByIdAsync(currentUser.UserId);
            var dto = new MessageDto(
                message.Id,
                message.ConversationId,
                message.SenderId,
                sender?.DisplayName ?? sender?.Email ?? "Unknown",
                sender?.AvatarUrl,
                message.Body,
                message.Type,
                message.Status,
                message.ReplyToId,
                message.IsDeleted,
                message.SentAt,
                true);

            await hubContext.Clients.Group($"conv-{id}").SendAsync("ReceiveMessage", dto, ct);
            ChatHub.BroadcastLocalMessage(dto);

            return Results.Created($"/api/v1/chat/conversations/{id}/messages/{message.Id}", dto);
        })
        .WithSummary("Kirim pesan ke percakapan");

        // ── Mark as read ─────────────────────────────────────────────────────
        group.MapPost("/conversations/{id:int}/read", async (
            int id,
            IChatService chat,
            CancellationToken ct) =>
        {
            await chat.MarkAsReadAsync(id, ct);
            return Results.Ok(new { Message = "Berhasil ditandai terbaca." });
        })
        .WithSummary("Tandai percakapan sebagai terbaca");

        // ── Search users ─────────────────────────────────────────────────────
        group.MapGet("/users/search", async (
            string q,
            IChatService chat,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return Results.Ok(Array.Empty<UserSearchResult>());

            var results = await chat.SearchUsersAsync(q.Trim(), ct);
            return Results.Ok(results);
        })
        .WithSummary("Cari pengguna berdasarkan nama atau email");
    }
}
