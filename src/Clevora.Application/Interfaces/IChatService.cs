using Clevora.Domain.Entities;
using Clevora.Shared.Models;

namespace Clevora.Application.Interfaces;

public interface IChatService
{
    Task<IReadOnlyList<ConversationSummary>> GetMyConversationsAsync(CancellationToken ct = default);
    Task<Conversation> StartPrivateChatAsync(string targetUserId, CancellationToken ct = default);
    Task<Conversation> CreateGroupAsync(string name, IEnumerable<string> memberIds, CancellationToken ct = default);
    Task<IReadOnlyList<Message>> GetMessagesAsync(int conversationId, int pageSize = 30, long? beforeId = null, CancellationToken ct = default);
    Task<Message> SendMessageAsync(int conversationId, string body, MessageType type = MessageType.Text, long? replyToId = null, CancellationToken ct = default);
    Task MarkAsReadAsync(int conversationId, CancellationToken ct = default);
    Task<IReadOnlyList<UserSearchResult>> SearchUsersAsync(string query, CancellationToken ct = default);
}
