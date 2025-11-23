namespace Jobsity.FinancialChat.Domain.Models;

public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public ChatRoom ChatRoom { get; set; }
    public Guid UserId { get; set; }
    public ChatUser User { get; set; }
    public string Text { get; set; }
    public DateTime TimeStamp { get; set; }
}
