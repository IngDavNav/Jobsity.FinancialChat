using AutoMapper;

using Jobsity.FinancialChat.Application.Abstractions;
using Jobsity.FinancialChat.Application.Abstractions.Persistence;
using Jobsity.FinancialChat.Application.ChatRooms;
using Jobsity.FinancialChat.Application.ChatRooms.Queries;
using Jobsity.FinancialChat.Domain.Models;

using Moq;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Xunit;
namespace Jobsity.FinancialChat.Tests.Application;
public class GetAllRoomsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IChatRoomRepository> _roomsRepoMock;
    private readonly Mock<IMapper> _mapperMock;

    public GetAllRoomsQueryHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _roomsRepoMock = new Mock<IChatRoomRepository>();
        _mapperMock = new Mock<IMapper>();

        _uowMock
            .SetupGet(u => u.Rooms)
            .Returns(_roomsRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoRoomsExist()
    {
        // Arrange
        _roomsRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ChatRoom>());

        _mapperMock
            .Setup(m => m.Map<IReadOnlyList<ChatRoomDto>>(It.IsAny<IReadOnlyList<ChatRoom>>()))
            .Returns(Array.Empty<ChatRoomDto>());

        var handler = new GetAllRoomsQueryHandler(_uowMock.Object, _mapperMock.Object);
        var query = new GetAllRoomsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _roomsRepoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<IReadOnlyList<ChatRoomDto>>(It.IsAny<IReadOnlyList<ChatRoom>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedDtos_WhenRoomsExist()
    {
        // Arrange
        var rooms = new List<ChatRoom>
        {
            new ChatRoom { Id = Guid.NewGuid(), Name = "General" },
            new ChatRoom { Id = Guid.NewGuid(), Name = "Finance" }
        };

        _roomsRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rooms);

        // Arrange
        var roomDtos = new List<ChatRoomDto>
        {
            new ChatRoomDto { Id = rooms[0].Id, Name = rooms[0].Name },
            new ChatRoomDto { Id = rooms[1].Id, Name = rooms[1].Name }
        };

        _mapperMock
            .Setup(m => m.Map<IReadOnlyList<ChatRoomDto>>(rooms))
            .Returns(roomDtos);

        var handler = new GetAllRoomsQueryHandler(_uowMock.Object, _mapperMock.Object);
        var query = new GetAllRoomsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Collection(result,
            r =>
            {
                Assert.Equal(rooms[0].Id, r.Id);
                Assert.Equal("General", r.Name);
            },
            r =>
            {
                Assert.Equal(rooms[1].Id, r.Id);
                Assert.Equal("Finance", r.Name);
            });

        _roomsRepoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<IReadOnlyList<ChatRoomDto>>(rooms), Times.Once);
    }
}
