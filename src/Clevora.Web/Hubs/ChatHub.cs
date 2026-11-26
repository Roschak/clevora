using System.Security.Claims;
using System.Text.Json;
using Clevora.Application.Interfaces;
using Clevora.Domain.Entities;
using Clevora.Infrastructure.Data;
using Clevora.Infrastructure.Identity;
using Clevora.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Clevora.Web.Hubs;

[Authorize]
public class ChatHub(
    IChatService chatService,
    UserManager<ClevoraUser> userManager,
    ClevoraDbContext db) : Hub
{
    private static readonly HashSet<string> OnlineUsers = new();
    private static readonly object _lock = new();

    public static event Action<MessageDto>? OnMessageBroadcast;
    public static void BroadcastLocalMessage(MessageDto dto) => OnMessageBroadcast?.Invoke(dto);

    private string UserId =>
        Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? Context.User?.FindFirstValue("sub")
        ?? throw new HubException("User tidak terautentikasi.");

    // ── Connection lifecycle ──────────────────────────────────────────────────
    public override async Task OnConnectedAsync()
    {
        var user = await userManager.FindByIdAsync(UserId);
        if (user is not null)
        {
            user.LastSeenAt = null; // online
            await userManager.UpdateAsync(user);
        }

        lock (_lock) { OnlineUsers.Add(UserId); }

        await Clients.All.SendAsync("UserOnline", UserId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = await userManager.FindByIdAsync(UserId);
        if (user is not null)
        {
            user.LastSeenAt = DateTime.UtcNow; // offline
            await userManager.UpdateAsync(user);
        }

        lock (_lock) { OnlineUsers.Remove(UserId); }

        await Clients.All.SendAsync("UserOffline", UserId, DateTime.UtcNow);
        await base.OnDisconnectedAsync(exception);
    }

    // ── Join/leave conversation groups ─────────────────────────────────────
    public async Task JoinConversation(int conversationId)
    {
        var isMember = await db.ConversationMembers
            .AnyAsync(m => m.ConversationId == conversationId && m.UserId == UserId);
        if (!isMember) throw new HubException("Anda bukan anggota percakapan ini.");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"conv-{conversationId}");
    }

    public async Task LeaveConversation(int conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conv-{conversationId}");
    }

    // ── Send message ─────────────────────────────────────────────────────────
    public async Task SendMessage(int conversationId, string body, long? replyToId)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new HubException("Pesan tidak boleh kosong.");

        var message = await chatService.SendMessageAsync(conversationId, body.Trim(), MessageType.Text, replyToId);

        var sender = await userManager.FindByIdAsync(UserId);
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
            IsMine: true);

        await Clients.Group($"conv-{conversationId}").SendAsync("ReceiveMessage", dto);
        OnMessageBroadcast?.Invoke(dto);
    }

    // ── Typing indicators ───────────────────────────────────────────────────
    public async Task StartTyping(int conversationId)
    {
        await Clients.OthersInGroup($"conv-{conversationId}")
            .SendAsync("UserTyping", UserId, conversationId);
    }

    public async Task StopTyping(int conversationId)
    {
        await Clients.OthersInGroup($"conv-{conversationId}")
            .SendAsync("UserStoppedTyping", UserId, conversationId);
    }

    // ── Presence query ───────────────────────────────────────────────────────
    public static bool IsUserOnline(string userId)
    {
        lock (_lock) { return OnlineUsers.Contains(userId); }
    }
}
