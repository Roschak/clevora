using Clevora.Domain.Entities;

namespace Clevora.Shared.Models;

public record MessageDto(
    long Id,
    int ConversationId,
    string SenderId,
    string SenderName,
    string? SenderAvatar,
    string Body,
    MessageType Type,
    MessageStatus Status,
    long? ReplyToId,
    bool IsDeleted,
    DateTime SentAt,
    bool IsMine);

public record ConversationDto(
    int Id,
    string Name,
    bool IsGroup,
    string? AvatarUrl,
    List<MessageDto> RecentMessages,
    int UnreadCount);

public record ConversationSummary(
    int Id,
    string Name,
    bool IsGroup,
    string? AvatarUrl,
    string? LastMessage,
    DateTime? LastMessageAt,
    int UnreadCount,
    bool IsOnline);

public record UserSearchResult(
    string Id,
    string DisplayName,
    string Email,
    string? AvatarUrl,
    bool IsOnline);

public record SendMessageRequest(
    int ConversationId,
    string Body,
    long? ReplyToId);

public record StartPrivateChatRequest(string TargetUserId);

public record CreateGroupRequest(string Name, List<string> MemberIds);
