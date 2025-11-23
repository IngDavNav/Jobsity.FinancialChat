using AutoMapper;

using Jobsity.FinancialChat.Application.Abstractions;
using Jobsity.FinancialChat.Application.Abstractions.Messaging;
using Jobsity.FinancialChat.Application.Abstractions.Persistence;
using Jobsity.FinancialChat.Application.Abstractions.Services;
using Jobsity.FinancialChat.Application.Bot.Contracts;
using Jobsity.FinancialChat.Application.Messages.Commands.SendMessage;
using Jobsity.FinancialChat.Application.Messages.Dtos;
using Jobsity.FinancialChat.Domain.Models;

using Moq;

using System;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace Jobsity.FinancialChat.Tests.Application;

public class SendMessageCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IChatUserRepository> _usersRepoMock;
    private readonly Mock<IChatRoomRepository> _roomsRepoMock;
    private readonly Mock<IChatMessageRepository> _messagesRepoMock;
    private readonly Mock<IChatNotificationService> _notificationServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IStockCommandBus> _stockCommandBusMock;

    public SendMessageCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _usersRepoMock = new Mock<IChatUserRepository>();
        _roomsRepoMock = new Mock<IChatRoomRepository>();
        _messagesRepoMock = new Mock<IChatMessageRepository>();
        _notificationServiceMock = new Mock<IChatNotificationService>();
        _mapperMock = new Mock<IMapper>();
        _stockCommandBusMock = new Mock<IStockCommandBus>();

        _uowMock.SetupGet(x => x.Users).Returns(_usersRepoMock.Object);
        _uowMock.SetupGet(x => x.Rooms).Returns(_roomsRepoMock.Object);
        _uowMock.SetupGet(x => x.Messages).Returns(_messagesRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var command = new SendMessageCommand(Guid.NewGuid(), Guid.NewGuid(), "hola");

        _usersRepoMock
            .Setup(r => r.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatUser)null!);

        var handler = new SendMessageCommandHandler(
            _uowMock.Object,
            _notificationServiceMock.Object,
            _mapperMock.Object,
            _stockCommandBusMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenRoomNotFound()
    {
        // Arrange
        var user = new ChatUser { Id = Guid.NewGuid(), UserName = "david" };
        var command = new SendMessageCommand(Guid.NewGuid(), user.Id, "hola");

        _usersRepoMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _roomsRepoMock
            .Setup(r => r.GetByIdAsync(command.RoomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatRoom)null!);

        var handler = new SendMessageCommandHandler(
            _uowMock.Object,
            _notificationServiceMock.Object,
            _mapperMock.Object,
            _stockCommandBusMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldEnqueueStockCommand_AndReturnNull_WhenTextIsStockCommand()
    {
        var user = new ChatUser { Id = Guid.NewGuid(), UserName = "david" };
        var room = new ChatRoom { Id = Guid.NewGuid(), Name = "General" };

        var command = new SendMessageCommand(room.Id, user.Id, "/stock=APPL.US");

        _usersRepoMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _roomsRepoMock
            .Setup(r => r.GetByIdAsync(room.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);


        var handler = new SendMessageCommandHandler(
            _uowMock.Object,
            _notificationServiceMock.Object,
            _mapperMock.Object,
            _stockCommandBusMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(result);
        _stockCommandBusMock.Verify(
            x => x.EnqueueAsync(It.IsAny<EnqueueBotStockCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _messagesRepoMock.Verify(x => x.AddAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _notificationServiceMock.Verify(x => x.NotifyMessageAsync(It.IsAny<ChatMessageDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPersistMessage_NotifyAndReturnDto_WhenNormalMessage()
    {
        var user = new ChatUser { Id = Guid.NewGuid(), UserName = "david" };
        var room = new ChatRoom { Id = Guid.NewGuid(), Name = "General" };
        var command = new SendMessageCommand(room.Id, user.Id, "hola mundo");

        _usersRepoMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _roomsRepoMock
            .Setup(r => r.GetByIdAsync(room.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        ChatMessage? capturedMessage = null;

        _messagesRepoMock
            .Setup(r => r.AddAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()))
            .Callback<ChatMessage, CancellationToken>((m, _) => capturedMessage = m)
            .Returns(Task.CompletedTask);

        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var dto = new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            UserId = user.Id,
            UserName = user.UserName!,
            Text = command.Text,
            TimeStamp = DateTime.UtcNow
        };

        _mapperMock
            .Setup(m => m.Map<ChatMessageDto>(It.IsAny<ChatMessage>()))
            .Returns(dto);

        _notificationServiceMock
            .Setup(n => n.NotifyMessageAsync(dto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new SendMessageCommandHandler(
            _uowMock.Object,
            _notificationServiceMock.Object,
            _mapperMock.Object,
            _stockCommandBusMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Id, result!.Id);
        Assert.Equal(dto.RoomId, result.RoomId);
        Assert.Equal(dto.UserId, result.UserId);
        Assert.Equal(dto.Text, result.Text);

        Assert.NotNull(capturedMessage);
        Assert.Equal(command.Text, capturedMessage!.Text);
        Assert.Equal(room.Id, capturedMessage.RoomId);
        Assert.Equal(user.Id, capturedMessage.UserId);

        _messagesRepoMock.Verify(x => x.AddAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(x => x.NotifyMessageAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
        _stockCommandBusMock.Verify(x => x.EnqueueAsync(It.IsAny<EnqueueBotStockCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
