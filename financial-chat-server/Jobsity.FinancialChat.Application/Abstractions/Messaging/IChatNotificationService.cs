using Jobsity.FinancialChat.Application.Bot.Dtos;
using Jobsity.FinancialChat.Application.Messages.Dtos;

namespace Jobsity.FinancialChat.Application.Abstractions.Messaging
{
    public interface IChatNotificationService
    {
        Task NotifyMessageAsync(ChatMessageDto message, CancellationToken ct = default);
        Task NotifyStockQuoteAsync(StockQuoteDto message, CancellationToken ct);
    }
}
