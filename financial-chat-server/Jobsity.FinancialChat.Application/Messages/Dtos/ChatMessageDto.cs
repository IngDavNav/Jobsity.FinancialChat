namespace Jobsity.FinancialChat.Application.Messages.Dtos;
public sealed class ChatMessageDto
{

    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Text { get; set; }
    public DateTime TimeStamp { get; set; }

}
