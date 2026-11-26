using Clevora.Domain.Entities;
using Clevora.Shared.Models;
using Xunit;

namespace Clevora.Mobile.Tests;

/// <summary>Chat-related tests for the mobile local database layer.</summary>
public class LocalMessageTests
{
    [Fact]
    public void MessageDto_IsMine_ReflectsCorrectSender()
    {
        // Arrange
        var myId = "user-123";
        var dto = new MessageDto(
            Id: 1,
            ConversationId: 10,
            SenderId: myId,
            SenderName: "Budi",
            SenderAvatar: null,
            Body: "Halo dunia!",
            Type: MessageType.Text,
            Status: MessageStatus.Sent,
            ReplyToId: null,
            IsDeleted: false,
            SentAt: DateTime.UtcNow,
            IsMine: true);

        Assert.True(dto.IsMine);
        Assert.Equal(myId, dto.SenderId);
    }

    [Fact]
    public void MessageDto_OtherSender_IsNotMine()
    {
        var dto = new MessageDto(
            Id: 2,
            ConversationId: 10,
            SenderId: "other-user",
            SenderName: "Siti",
            SenderAvatar: null,
            Body: "Balas pesan",
            Type: MessageType.Text,
            Status: MessageStatus.Delivered,
            ReplyToId: null,
            IsDeleted: false,
            SentAt: DateTime.UtcNow,
            IsMine: false);

        Assert.False(dto.IsMine);
        Assert.Equal(MessageStatus.Delivered, dto.Status);
    }

    [Fact]
    public void MessageDto_ReplyTo_LinkedCorrectly()
    {
        var parent = new MessageDto(1, 5, "u1", "Alice", null, "Pesan asli", MessageType.Text, MessageStatus.Read, null, false, DateTime.UtcNow, false);
        var reply = new MessageDto(2, 5, "u2", "Bob", null, "Ini balasan", MessageType.Text, MessageStatus.Sent, 1, false, DateTime.UtcNow, false);

        Assert.Equal(parent.Id, reply.ReplyToId);
    }

    [Fact]
    public void ConversationSummary_UnreadCount_CorrectlyStored()
    {
        var summary = new ConversationSummary(
            Id: 1,
            Name: "Team Clevora",
            IsGroup: true,
            AvatarUrl: null,
            LastMessage: "Build selesai!",
            LastMessageAt: DateTime.UtcNow,
            UnreadCount: 3,
            IsOnline: false);

        Assert.Equal(3, summary.UnreadCount);
        Assert.True(summary.IsGroup);
    }
}
