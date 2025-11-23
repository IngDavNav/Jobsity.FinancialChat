using AutoMapper;

using Jobsity.FinancialChat.Application.Abstractions;
using Jobsity.FinancialChat.Application.Abstractions.Messaging;
using Jobsity.FinancialChat.Application.Abstractions.Persistence;
using Jobsity.FinancialChat.Application.Bot.Contracts;
using Jobsity.FinancialChat.Application.Messages.Dtos;
using Jobsity.FinancialChat.Bot.Worker.RabbitMqConfigurations;
using Jobsity.FinancialChat.Domain.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using RabbitMQ.Client;

using System.Reflection;

using Xunit;

public class BotMessagesConsumerTests
{
    private readonly Mock<ILogger<BotMessagesConsumer>> _loggerMock = new();
    private readonly Mock<IServiceProvider> _rootProviderMock = new();
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock = new();
    private readonly Mock<IServiceScope> _scopeMock = new();
    private readonly Mock<IServiceProvider> _scopedProviderMock = new();

    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IChatUserRepository> _usersRepoMock = new();
    private readonly Mock<IChatRoomRepository> _roomsRepoMock = new();
    private readonly Mock<IChatMessageRepository> _messagesRepoMock = new();
    private readonly Mock<IChatNotificationService> _notificationServiceMock = new();

    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IRabbitMqConnectionFactory> _connectionFactoryMock = new();
    private readonly Mock<IConnection> _connectionMock = new();
    private readonly Mock<IModel> _channelMock = new();

    private readonly IOptions<RabbitMqOptions> _options;

    public BotMessagesConsumerTests()
    {
        _options = Options.Create(new RabbitMqOptions
        {
            BotMessagesQueue = "bot.queue",
            BotUserName = "stock-bot"
        });

        _rootProviderMock
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(_scopeFactoryMock.Object);

        _scopeFactoryMock
            .Setup(sf => sf.CreateScope())
            .Returns(_scopeMock.Object);

        _scopeMock
            .SetupGet(s => s.ServiceProvider)
            .Returns(_scopedProviderMock.Object);

        _scopedProviderMock
            .Setup(sp => sp.GetService(typeof(IUnitOfWork)))
            .Returns(_uowMock.Object);

        _scopedProviderMock
            .Setup(sp => sp.GetService(typeof(IChatNotificationService)))
            .Returns(_notificationServiceMock.Object);

        _uowMock.SetupGet(x => x.Users).Returns(_usersRepoMock.Object);
        _uowMock.SetupGet(x => x.Rooms).Returns(_roomsRepoMock.Object);
        _uowMock.SetupGet(x => x.Messages).Returns(_messagesRepoMock.Object);

        _connectionFactoryMock
            .Setup(f => f.CreateConnection())
            .Returns(_connectionMock.Object);

        _connectionMock
            .Setup(c => c.CreateModel())
            .Returns(_channelMock.Object);

        _channelMock
            .Setup(c => c.QueueDeclare(
                It.IsAny<string>(),
                true, false, false, null));
    }

    private BotMessagesConsumer CreateSutWithExistingBotUser()
    {
        var existingBot = new ChatUser
        {
            Id = Guid.NewGuid(),
            UserName = _options.Value.BotUserName
        };

        _usersRepoMock
            .Setup(r => r.GetByUserNameAsync(existingBot.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBot);

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

        _usersRepoMock.Setup(r => r.AddAsync(It.IsAny<ChatUser>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var sut = new BotMessagesConsumer(
            _loggerMock.Object,
            _rootProviderMock.Object,
            _mapperMock.Object,
            _connectionFactoryMock.Object,
            _options);

        return sut;
    }

    private async Task InvokeHandleBotMessageAsync(
        BotMessagesConsumer sut,
        BotChatMessage payload,
        CancellationToken ct)
    {
        var method = typeof(BotMessagesConsumer)
            .GetMethod("HandleBotMessageAsync", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(method); 

        var task = (Task)method!.Invoke(sut, new object[] { payload, ct })!;
        await task;
    }

    [Fact]
    public void Ctor_ShouldNotCreateBot_WhenBotUserAlreadyExists()
    {
        // Arrange
        var existingBot = new ChatUser
        {
            Id = Guid.NewGuid(),
            UserName = _options.Value.BotUserName
        };

        _usersRepoMock
            .Setup(r => r.GetByUserNameAsync(_options.Value.BotUserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBot);

        _usersRepoMock
            .Setup(r => r.AddAsync(It.IsAny<ChatUser>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var _ = new BotMessagesConsumer(
            _loggerMock.Object,
            _rootProviderMock.Object,
            _mapperMock.Object,
            _connectionFactoryMock.Object,
            _options);

        // Assert
        _usersRepoMock.Verify(
            r => r.AddAsync(It.IsAny<ChatUser>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _uowMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void Ctor_ShouldCreateBot_WhenBotUserDoesNotExist()
    {
        // Arrange
        _usersRepoMock
            .Setup(r => r.GetByUserNameAsync(_options.Value.BotUserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatUser)null!);

        ChatUser? createdBot = null;

        _usersRepoMock
            .Setup(r => r.AddAsync(It.IsAny<ChatUser>(), It.IsAny<CancellationToken>()))
            .Callback<ChatUser, CancellationToken>((u, _) => createdBot = u)
            .Returns(Task.CompletedTask);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var _ = new BotMessagesConsumer(
            _loggerMock.Object,
            _rootProviderMock.Object,
            _mapperMock.Object,
            _connectionFactoryMock.Object,
            _options);

        // Assert
        Assert.NotNull(createdBot);
        Assert.Equal(_options.Value.BotUserName, createdBot!.UserName);

        _usersRepoMock.Verify(
            r => r.AddAsync(It.IsAny<ChatUser>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _uowMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task HandleBotMessageAsync_ShouldDoNothing_WhenRoomNotFound()
    {
        // Arrange
        var sut = CreateSutWithExistingBotUser();

        var payload = new BotChatMessage
        {
            RoomId = Guid.NewGuid(),
            Text = "hola desde bot"
        };

        _roomsRepoMock
            .Setup(r => r.GetByIdAsync(payload.RoomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatRoom)null!);

        // Act
        await InvokeHandleBotMessageAsync(sut, payload, CancellationToken.None).ConfigureAwait(false);

        // Assert
        _messagesRepoMock.Verify(
            r => r.AddAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _uowMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _notificationServiceMock.Verify(
            n => n.NotifyMessageAsync(It.IsAny<ChatMessageDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task HandleBotMessageAsync_ShouldPersistAndNotify_WhenRoomExists()
    {
        // Arrange
        var sut = CreateSutWithExistingBotUser();

        var room = new ChatRoom
        {
            Id = Guid.NewGuid(),
            Name = "General"
        };

        var payload = new BotChatMessage
        {
            RoomId = room.Id,
            Text = "COTIZACIÓN AAPL.US 190.32"
        };

        _roomsRepoMock
            .Setup(r => r.GetByIdAsync(room.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        ChatMessage? capturedMessage = null;

        _messagesRepoMock
            .Setup(r => r.AddAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()))
            .Callback<ChatMessage, CancellationToken>((m, _) => capturedMessage = m)
            .Returns(Task.CompletedTask);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var dto = new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            UserId = Guid.NewGuid(),
            UserName = "stock-bot",
            Text = payload.Text,
            TimeStamp = DateTime.UtcNow
        };

        _mapperMock
            .Setup(m => m.Map<ChatMessageDto>(It.IsAny<ChatMessage>()))
            .Returns(dto);

        _notificationServiceMock
            .Setup(n => n.NotifyMessageAsync(dto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await InvokeHandleBotMessageAsync(sut, payload, CancellationToken.None).ConfigureAwait(false);

        // Assert
        Assert.NotNull(capturedMessage);
        Assert.Equal(room.Id, capturedMessage!.RoomId);
        Assert.Equal(payload.Text, capturedMessage.Text);
        Assert.NotEqual(default, capturedMessage.Id);

        _messagesRepoMock.Verify(
            r => r.AddAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _uowMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _notificationServiceMock.Verify(
            n => n.NotifyMessageAsync(It.IsAny<ChatMessageDto>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}


