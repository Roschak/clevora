using Clevora.Application.Interfaces;
using Clevora.Domain.Entities;
using Clevora.Infrastructure.Data;
using Clevora.Infrastructure.Identity;
using Clevora.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clevora.Infrastructure.Services;

public class ChatService(
    ClevoraDbContext db,
    ICurrentUserService currentUser,
    UserManager<ClevoraUser> userManager) : IChatService
{
    // ── Get all conversations the current user is a member of ─────────────────
    public async Task<IReadOnlyList<ConversationSummary>> GetMyConversationsAsync(CancellationToken ct = default)
    {
        var userId = currentUser.UserId;

        var memberConvIds = await db.ConversationMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.ConversationId)
            .ToListAsync(ct);

        var conversations = await db.Conversations
            .Where(c => memberConvIds.Contains(c.Id))
            .Include(c => c.Members)
            .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .ToListAsync(ct);

        var summaries = new List<ConversationSummary>();

        foreach (var conv in conversations)
        {
            // For 1-on-1 chats, resolve the other user's display name
            string displayName = conv.Name ?? string.Empty;
            string? avatarUrl = conv.AvatarUrl;
            bool isOnline = false;

            if (!conv.IsGroup)
            {
                var otherMember = conv.Members.FirstOrDefault(m => m.UserId != userId);
                if (otherMember is not null)
                {
                    var otherUser = await userManager.FindByIdAsync(otherMember.UserId);
                    if (otherUser is not null)
                    {
                        displayName = otherUser.DisplayName ?? otherUser.Email ?? otherMember.UserId;
                        avatarUrl ??= otherUser.AvatarUrl;
                        isOnline = otherUser.LastSeenAt is null;
                    }
                }
            }

            var myMembership = conv.Members.FirstOrDefault(m => m.UserId == userId);
            var lastReadAt = myMembership?.LastReadAt;

            // Count unread messages
            int unread = lastReadAt is null
                ? 0
                : await db.Messages
                    .CountAsync(m => m.ConversationId == conv.Id
                                     && m.SenderId != userId
                                     && m.SentAt > lastReadAt, ct);

            var lastMsg = conv.Messages.FirstOrDefault();
            summaries.Add(new ConversationSummary(
                conv.Id,
                displayName,
                conv.IsGroup,
                avatarUrl,
                lastMsg?.IsDeleted == true ? "(pesan dihapus)" : lastMsg?.Body,
                lastMsg?.SentAt,
                unread,
                isOnline));
        }

        return summaries.OrderByDescending(s => s.LastMessageAt).ToList();
    }

    // ── Start or reuse a 1-on-1 private chat ──────────────────────────────────
    public async Task<Conversation> StartPrivateChatAsync(string targetUserId, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;

        // Check if a 1-on-1 conversation already exists between the two users
        var existing = await db.ConversationMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.ConversationId)
            .ToListAsync(ct);

        var targetConvIds = await db.ConversationMembers
            .Where(m => m.UserId == targetUserId)
            .Select(m => m.ConversationId)
            .ToListAsync(ct);

        var sharedIds = existing.Intersect(targetConvIds).ToList();

        if (sharedIds.Count > 0)
        {
            var existingConv = await db.Conversations
                .FirstOrDefaultAsync(c => sharedIds.Contains(c.Id) && !c.IsGroup, ct);
            if (existingConv is not null)
                return existingConv;
        }

        // Create new 1-on-1 conversation
        var conversation = new Conversation
        {
            IsGroup = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Conversations.Add(conversation);
        await db.SaveChangesAsync(ct);

        db.ConversationMembers.Add(new ConversationMember { ConversationId = conversation.Id, UserId = userId });
        db.ConversationMembers.Add(new ConversationMember { ConversationId = conversation.Id, UserId = targetUserId });
        await db.SaveChangesAsync(ct);

        return conversation;
    }

    // ── Create a group conversation ───────────────────────────────────────────
    public async Task<Conversation> CreateGroupAsync(string name, IEnumerable<string> memberIds, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        var allMembers = memberIds.Append(userId).Distinct().ToList();

        var conversation = new Conversation
        {
            Name = name,
            IsGroup = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Conversations.Add(conversation);
        await db.SaveChangesAsync(ct);

        foreach (var memberId in allMembers)
        {
            db.ConversationMembers.Add(new ConversationMember
            {
                ConversationId = conversation.Id,
                UserId = memberId
            });
        }
        await db.SaveChangesAsync(ct);

        return conversation;
    }

    // ── Get paginated messages for a conversation ─────────────────────────────
    public async Task<IReadOnlyList<Message>> GetMessagesAsync(
        int conversationId,
        int pageSize = 30,
        long? beforeId = null,
        CancellationToken ct = default)
    {
        var userId = currentUser.UserId;

        // Verify membership
        var isMember = await db.ConversationMembers
            .AnyAsync(m => m.ConversationId == conversationId && m.UserId == userId, ct);
        if (!isMember) return [];

        var query = db.Messages
            .Where(m => m.ConversationId == conversationId);

        if (beforeId.HasValue)
            query = query.Where(m => m.Id < beforeId.Value);

        return await query
            .OrderByDescending(m => m.SentAt)
            .Take(pageSize)
            .OrderBy(m => m.SentAt)
            .ToListAsync(ct);
    }

    // ── Send a message ────────────────────────────────────────────────────────
    public async Task<Message> SendMessageAsync(
        int conversationId,
        string body,
        MessageType type = MessageType.Text,
        long? replyToId = null,
        CancellationToken ct = default)
    {
        var userId = currentUser.UserId;

        // Verify membership
        var isMember = await db.ConversationMembers
            .AnyAsync(m => m.ConversationId == conversationId && m.UserId == userId, ct);
        if (!isMember)
            throw new UnauthorizedAccessException("Anda bukan anggota percakapan ini.");

        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = userId,
            Body = body,
            Type = type,
            Status = MessageStatus.Sent,
            ReplyToId = replyToId,
            SentAt = DateTime.UtcNow
        };

        db.Messages.Add(message);

        // Update conversation's UpdatedAt for ordering
        var conversation = await db.Conversations.FindAsync([conversationId], ct);
        if (conversation is not null)
            conversation.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return message;
    }

    // ── Mark messages as read ─────────────────────────────────────────────────
    public async Task MarkAsReadAsync(int conversationId, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        var membership = await db.ConversationMembers
            .FirstOrDefaultAsync(m => m.ConversationId == conversationId && m.UserId == userId, ct);

        if (membership is not null)
        {
            membership.LastReadAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }

    // ── Search users ──────────────────────────────────────────────────────────
    public async Task<IReadOnlyList<UserSearchResult>> SearchUsersAsync(string query, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        var lower = query.ToLowerInvariant();

        var users = await userManager.Users
            .Where(u => u.Id != userId
                        && (u.DisplayName != null && u.DisplayName.ToLower().Contains(lower)
                            || u.Email != null && u.Email.ToLower().Contains(lower)))
            .Take(20)
            .ToListAsync(ct);

        return users.Select(u => new UserSearchResult(
            u.Id,
            u.DisplayName ?? u.Email ?? u.Id,
            u.Email ?? string.Empty,
            u.AvatarUrl,
            u.LastSeenAt is null)).ToList();
    }
}
