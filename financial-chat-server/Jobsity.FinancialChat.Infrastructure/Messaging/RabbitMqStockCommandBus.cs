using Jobsity.FinancialChat.Application.Abstractions.Services;
using Jobsity.FinancialChat.Application.Bot.Contracts;
using Jobsity.FinancialChat.Bot.Worker.RabbitMqConfigurations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;

using System.Text;
using System.Text.Json;

namespace Jobsity.FinancialChat.Infrastructure.Messaging;

public sealed class RabbitMqStockCommandBus : IStockCommandBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMqOptions _options;


    public RabbitMqStockCommandBus(
        IRabbitMqConnectionFactory connectionFactory,
        IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password
        };

        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public Task EnqueueAsync(EnqueueBotStockCommand command, CancellationToken ct = default)
    {
        var payload = JsonSerializer.Serialize(command);
        var body = Encoding.UTF8.GetBytes(payload);

        var props = _channel.CreateBasicProperties();
        props.Persistent = true;

        _channel.BasicPublish(
            exchange: "",
            routingKey: _options.QueueName,
            basicProperties: props,
            body: body);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
