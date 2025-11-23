namespace Jobsity.FinancialChat.Domain.ValueObjects;

public class StockQuote
{
    public string StockCode { get; }
    public decimal Price { get; }
    public bool IsValid { get; }
    public string? ErrorMessage { get; }

    private StockQuote(string stockCode, decimal price, bool isValid, string? error)
    {
        StockCode = stockCode;
        Price = price;
        IsValid = isValid;
        ErrorMessage = error;
    }

    public static StockQuote Success(string stockCode, decimal price) =>
        new(stockCode, price, true, null);

    public static StockQuote Error(string stockCode, string message) =>
        new(stockCode, 0m, false, message);
}
