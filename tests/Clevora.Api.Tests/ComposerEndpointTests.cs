using Clevora.Domain.Entities;
using Clevora.Shared.Models;
using Xunit;

namespace Clevora.Api.Tests;

/// <summary>Tests for chat API contracts and DTOs.</summary>
public class ChatEndpointTests
{
    [Fact]
    public void SendMessageRequest_Constructed_Correctly()
    {
        var req = new SendMessageRequest(1, "Halo!", null);

        Assert.Equal(1, req.ConversationId);
        Assert.Equal("Halo!", req.Body);
        Assert.Null(req.ReplyToId);
    }

    [Fact]
    public void StartPrivateChatRequest_Constructed_Correctly()
    {
        var req = new StartPrivateChatRequest("target-user-id");

        Assert.Equal("target-user-id", req.TargetUserId);
    }

    [Fact]
    public void CreateGroupRequest_IncludesMembers()
    {
        var req = new CreateGroupRequest("Team Clevora", new List<string> { "u1", "u2", "u3" });

        Assert.Equal("Team Clevora", req.Name);
        Assert.Equal(3, req.MemberIds.Count);
    }

    [Fact]
    public void MessageDto_Statuses_CoverAllStates()
    {
        var statuses = new[] { MessageStatus.Sent, MessageStatus.Delivered, MessageStatus.Read };
        Assert.Equal(3, statuses.Length);
        Assert.Contains(MessageStatus.Sent, statuses);
    }

    [Fact]
    public void MessageType_CoversChatTypes()
    {
        var types = new[] { MessageType.Text, MessageType.Image, MessageType.File, MessageType.System };
        Assert.Equal(4, types.Length);
    }
}
