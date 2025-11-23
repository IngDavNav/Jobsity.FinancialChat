namespace Jobsity.FinancialChat.Application.Bot.Contracts;

public sealed class BotChatMessage
{
    public Guid RoomId { get; set; }
    public string Text { get; set; } = default!;
}
