using Jobsity.FinancialChat.Application.Bot.Dtos;
using Jobsity.FinancialChat.Application.Messages.Dtos;

public interface IChatClient
{
    Task ReceiveMessage(ChatMessageDto message);
    Task ReceiveStockQuote(StockQuoteDto message);
}
