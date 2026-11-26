namespace Clevora.Domain.Entities;

public enum MessageType { Text, Image, File, System }
public enum MessageStatus { Sent, Delivered, Read }

public class Message
{
    public long Id { get; set; }
    public int ConversationId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;
    public MessageStatus Status { get; set; } = MessageStatus.Sent;
    public long? ReplyToId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
    public Message? ReplyTo { get; set; }
}
