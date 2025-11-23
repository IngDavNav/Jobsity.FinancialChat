using Jobsity.FinancialChat.Application.Bot.Contracts;
using Jobsity.FinancialChat.Bot.Worker.RabbitMqConfigurations;

using Microsoft.Extensions.Options;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System.Text;
using System.Text.Json;

namespace Jobsity.FinancialChat.Bot.Worker;

public sealed class StockBotWorker : BackgroundService
{
    private readonly ILogger<StockBotWorker> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly HttpClient _httpClient;
    private readonly RabbitMqOptions _options;

    private const string CommandQueue = "stock-commands";
    private const string BotMessagesQueue = "bot-messages";

    public StockBotWorker(
        ILogger<StockBotWorker> logger,
        IRabbitMqConnectionFactory connectionFactory,
        IOptions<RabbitMqOptions> options,
        HttpClient httpClient)
    {
        _logger = logger;
        _options = options.Value;
        _httpClient = httpClient;

        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueDeclare(
            _options.BotMessagesQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var cmd = JsonSerializer.Deserialize<EnqueueBotStockCommand>(json);

                if (cmd == null)
                {
                    _logger.LogWarning("Invalid command payload: {Payload}", json);
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    return;
                }

                var messageText = await ProcessStockCommandAsync(cmd, stoppingToken).ConfigureAwait(false);

                var botMessage = new BotChatMessage
                {
                    RoomId = cmd.RoomId,
                    Text = messageText
                };

                var outJson = JsonSerializer.Serialize(botMessage);
                var outBody = Encoding.UTF8.GetBytes(outJson);

                var props = _channel.CreateBasicProperties();
                props.Persistent = true;

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: _options.BotMessagesQueue,
                    basicProperties: props,
                    body: outBody);

                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing stock command");
                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
        };

        _channel.BasicConsume(
            queue: _options.QueueName,
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }

    private async Task<string> ProcessStockCommandAsync(
        EnqueueBotStockCommand cmd,
        CancellationToken ct)
    {
        var stockCode = cmd.StockCode; // ej. "aapl.us"
        var url = $"https://stooq.com/q/l/?s={stockCode}&f=sd2t2ohlcv&h&e=csv";

        try
        {
            var csv = await _httpClient.GetStringAsync(url, ct).ConfigureAwait(false);

            var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
            {
                return $"Sorry, I couldn't retrieve a quote for {stockCode}.";
            }

            var data = lines[1].Split(',');
            var symbol = data[0];
            var close = data[6];

            if (string.Equals(close, "N/D", StringComparison.OrdinalIgnoreCase))
            {
                return $"Sorry, I couldn't find a valid quote for {symbol}.";
            }

            return $"{symbol.ToUpperInvariant()} quote is ${close} per share";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Stooq for {StockCode}", stockCode);
            return $"Sorry, an error occurred while retrieving the quote for {stockCode}.";
        }
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}

