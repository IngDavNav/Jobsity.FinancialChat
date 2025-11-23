using AutoMapper;

using Jobsity.FinancialChat.Application.Abstractions;
using Jobsity.FinancialChat.Application.Abstractions.Messaging;
using Jobsity.FinancialChat.Application.Bot.Contracts;
using Jobsity.FinancialChat.Application.Messages.Dtos;
using Jobsity.FinancialChat.Bot.Worker.RabbitMqConfigurations;
using Jobsity.FinancialChat.Domain.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System.Text;
using System.Text.Json;

public sealed class BotMessagesConsumer : BackgroundService
{
    private readonly ILogger<BotMessagesConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMapper _mapper;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMqOptions _options;

    private readonly ChatUser _botUser;

    public BotMessagesConsumer(
        ILogger<BotMessagesConsumer> logger,
        IServiceProvider serviceProvider,
        IMapper mapper,
        IRabbitMqConnectionFactory connectionFactory,
        IOptions<RabbitMqOptions> options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _mapper = mapper;
        _options = options.Value;

        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(_options.BotMessagesQueue, true, false, false, null);

        _botUser = EnsureBotUserExists();
    }

    private ChatUser EnsureBotUserExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();



        var existing = uow.Users.GetByUserNameAsync(_options.BotUserName).Result;

        if (existing != null)
            return existing;

        var bot = new ChatUser
        {
            Id = new Guid(),
            UserName = _options.BotUserName
        };

        uow.Users.AddAsync(bot).Wait();
        uow.SaveChangesAsync().Wait();

        return bot;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var payload = JsonSerializer.Deserialize<BotChatMessage>(json);

                if (payload == null)
                {
                    _logger.LogWarning("Invalid bot message payload: {Payload}", json);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                await HandleBotMessageAsync(payload, stoppingToken).ConfigureAwait(false);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bot message");
                _channel.BasicAck(ea.DeliveryTag, false);
            }
        };

        _channel.BasicConsume(_options.BotMessagesQueue, false, consumer);

        return Task.CompletedTask;
    }

    private async Task HandleBotMessageAsync(BotChatMessage payload, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var notificationService = scope.ServiceProvider.GetRequiredService<IChatNotificationService>();

        var room = await uow.Rooms.GetByIdAsync(payload.RoomId, ct).ConfigureAwait(false);
        if (room == null)
        {
            _logger.LogWarning("Room {RoomId} not found", payload.RoomId);
            return;
        }
                
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            UserId = _botUser.Id,
            Text = payload.Text,
            TimeStamp = DateTime.UtcNow
        };

        await uow.Messages.AddAsync(message, ct).ConfigureAwait(false);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);

        var messageDto = _mapper.Map<ChatMessageDto>(message);
        messageDto.UserName = _botUser.UserName;

        await notificationService
            .NotifyMessageAsync(messageDto, ct)
            .ConfigureAwait(false);
    }

    public override void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
        base.Dispose();
    }
}
