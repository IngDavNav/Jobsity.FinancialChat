using Jobsity.FinancialChat.Application.Abstractions.Messaging;
using Jobsity.FinancialChat.Application.Bot.Dtos;
using Jobsity.FinancialChat.Application.Messages.Dtos;

using Microsoft.AspNetCore.SignalR;

namespace Jobsity.FinancialChat.Api.RealTime;
public class SignalRChatNotificationService : IChatNotificationService
{
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;

    public SignalRChatNotificationService(
        IHubContext<ChatHub, IChatClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyMessageAsync(ChatMessageDto message, CancellationToken ct = default)
       => _hubContext
           .Clients
           .Group(message.RoomId.ToString())
           .ReceiveMessage(message);

    public Task NotifyStockQuoteAsync(StockQuoteDto quote, CancellationToken ct = default)
        => _hubContext
            .Clients
            .Group(quote.RoomId.ToString())
            .ReceiveStockQuote(quote);
}
