using AutoMapper;

using Jobsity.FinancialChat.Application.Abstractions;
using Jobsity.FinancialChat.Application.Abstractions.Persistence;
using Jobsity.FinancialChat.Application.ChatRooms;
using Jobsity.FinancialChat.Application.ChatRooms.Commands.CreateChatRoom;
using Jobsity.FinancialChat.Domain.Models;

using Moq;

namespace Jobsity.FinancialChat.Tests.Application;

public class CreateRoomCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IChatRoomRepository> _roomsRepoMock;
    private readonly Mock<IMapper> _mapperMock;

    public CreateRoomCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _roomsRepoMock = new Mock<IChatRoomRepository>();
        _mapperMock = new Mock<IMapper>();

        _uowMock
            .SetupGet(u => u.Rooms)
            .Returns(_roomsRepoMock.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_ShouldThrowArgumentException_WhenNameIsNullOrWhiteSpace(string? name)
    {
        // Arrange
        var handler = new CreateRoomCommandHandler(_uowMock.Object, _mapperMock.Object);
        var command = new CreateRoomCommand(name);

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(act);
        Assert.Equal("Room name is required.", ex.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenRoomAlreadyExists()
    {
        // Arrange
        var command = new CreateRoomCommand("General");

        _roomsRepoMock
            .Setup(r => r.ExistsByNameAsync("General", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateRoomCommandHandler(_uowMock.Object, _mapperMock.Object);

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Equal("Room 'General' already exists.", ex.Message);
    }

    [Fact]
    public async Task Handle_ShouldCreateRoomAndReturnDto_WhenNameIsValidAndNotExists()
    {
        // Arrange
        var command = new CreateRoomCommand("General");

        _roomsRepoMock
            .Setup(r => r.ExistsByNameAsync("General", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        ChatRoom? capturedRoom = null;

        _roomsRepoMock
            .Setup(r => r.AddAsync(It.IsAny<ChatRoom>(), It.IsAny<CancellationToken>()))
            .Callback<ChatRoom, CancellationToken>((room, _) => capturedRoom = room)
            .Returns(Task.CompletedTask);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var dto = new ChatRoomDto
        {
            Id = Guid.NewGuid(),
            Name = "General"
        };

        _mapperMock
            .Setup(m => m.Map<ChatRoomDto>(It.IsAny<ChatRoom>()))
            .Returns(dto);

        var handler = new CreateRoomCommandHandler(_uowMock.Object, _mapperMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRoom);
        Assert.Equal("General", capturedRoom!.Name);

        _roomsRepoMock.Verify(r => r.AddAsync(It.IsAny<ChatRoom>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        Assert.Equal(dto.Id, result.Id);
        Assert.Equal(dto.Name, result.Name);
    }
}
