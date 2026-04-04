using MessagePack;

namespace AbujaSocialMetaverse.Infrastructure.RealTime.Models;

[MessagePackObject]
public class ChatMessage
{
    [Key(0)] public Guid MessageId { get; set; } = Guid.NewGuid();
    [Key(1)] public Guid SenderId { get; set; }
    [Key(2)] public Guid SessionId { get; set; }
    [Key(3)] public string Content { get; set; } = string.Empty;
    [Key(4)] public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;
    [Key(5)] public ChatMessageType Type { get; set; } = ChatMessageType.Text;
}

public enum ChatMessageType
{
    Text = 0,
    System = 1,
    MatchNotification = 2
}