using AutoMapper;

using Jobsity.FinancialChat.Application.Abstractions;
using Jobsity.FinancialChat.Application.Abstractions.Persistence;
using Jobsity.FinancialChat.Application.Messages.Dtos;
using Jobsity.FinancialChat.Application.Messages.Queries;
using Jobsity.FinancialChat.Domain.Models;

using Moq;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace Jobsity.FinancialChat.Tests.Application;

public class GetMessagesByRoomQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IChatMessageRepository> _messagesRepoMock;
    private readonly Mock<IMapper> _mapperMock;

    public GetMessagesByRoomQueryHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _messagesRepoMock = new Mock<IChatMessageRepository>();
        _mapperMock = new Mock<IMapper>();

        _uowMock.SetupGet(x => x.Messages).Returns(_messagesRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoMessagesFound()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var query = new GetMessagesByRoomQuery { RoomId = roomId };

        _messagesRepoMock
            .Setup(r => r.GetLastMessagesAsync(roomId, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChatMessage>());

        _mapperMock
            .Setup(m => m.Map<IEnumerable<ChatMessageDto>>(It.IsAny<IEnumerable<ChatMessage>>()))
            .Returns(new List<ChatMessageDto>());

        var handler = new GetMessagesByRoomQueryHandler(_uowMock.Object, _mapperMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _messagesRepoMock.Verify(
            r => r.GetLastMessagesAsync(roomId, 50, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _mapperMock.Verify(
            m => m.Map<IEnumerable<ChatMessageDto>>(It.IsAny<IEnumerable<ChatMessage>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedDtos_WhenMessagesExist()
    {
        var roomId = Guid.NewGuid();

        var messages = new List<ChatMessage>
        {
            new ChatMessage
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                UserId = Guid.NewGuid(),
                Text = "Hola",
                TimeStamp = DateTime.UtcNow
            },
            new ChatMessage
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                UserId = Guid.NewGuid(),
                Text = "Mundo",
                TimeStamp = DateTime.UtcNow
            }
        };

        var dtos = new List<ChatMessageDto>
        {
            new ChatMessageDto
            {
                Id = messages[0].Id,
                RoomId = roomId,
                UserId = messages[0].UserId,
                UserName = "alice",
                Text = messages[0].Text,
                TimeStamp = messages[0].TimeStamp
            },
            new ChatMessageDto
            {
                Id = messages[1].Id,
                RoomId = roomId,
                UserId = messages[1].UserId,
                UserName = "bob",
                Text = messages[1].Text,
                TimeStamp = messages[1].TimeStamp
            }
        };

        _messagesRepoMock
            .Setup(r => r.GetLastMessagesAsync(roomId, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<ChatMessageDto>>(messages))
            .Returns(dtos);

        var handler = new GetMessagesByRoomQueryHandler(_uowMock.Object, _mapperMock.Object);

        // Act
        var result = await handler.Handle(
            new GetMessagesByRoomQuery { RoomId = roomId },
            CancellationToken.None
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, dtos.Count);

        var list = new List<ChatMessageDto>(result);

        Assert.Equal(messages[0].Id, list[0].Id);
        Assert.Equal(messages[0].Text, list[0].Text);

        Assert.Equal(messages[1].Id, list[1].Id);
        Assert.Equal(messages[1].Text, list[1].Text);

        _messagesRepoMock.Verify(
            r => r.GetLastMessagesAsync(roomId, 50, It.IsAny<CancellationToken>()),
            Times.Once);

        _mapperMock.Verify(
            m => m.Map<IEnumerable<ChatMessageDto>>(messages),
            Times.Once);
    }
}
