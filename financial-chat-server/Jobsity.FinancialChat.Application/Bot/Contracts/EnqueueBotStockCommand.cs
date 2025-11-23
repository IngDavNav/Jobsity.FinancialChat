namespace Jobsity.FinancialChat.Application.Bot.Contracts;

public sealed record EnqueueBotStockCommand(Guid RoomId, string StockCode)
{
    private const string Prefix = "/stock=";

    public static bool TryParse(Guid roomId, string text, out EnqueueBotStockCommand? command)
    {
        command = null;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (!text.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        var symbol = text[Prefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(symbol))
            return false;

        command = new EnqueueBotStockCommand(roomId, symbol);
        return true;
    }
}
