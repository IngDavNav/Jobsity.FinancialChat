using System;
using System.Threading;
using System.Threading.Tasks;
using Jobsity.FinancialChat.Application.Auth.Commands.RegisterUser;
using Jobsity.FinancialChat.Application.Auth.Dtos;
using Jobsity.FinancialChat.Application.Abstractions.Auth;
using Jobsity.FinancialChat.Application.Common.Identity;
using Jobsity.FinancialChat.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using Jobsity.FinancialChat.Application.Abstractions;


namespace Jobsity.FinancialChat.Tests.Application;
public class RegisterUserCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IJwtTokenGenerator> _jwtMock;
    private readonly Mock<IUnitOfWork> _uowMock;

    public RegisterUserCommandHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        _jwtMock = new Mock<IJwtTokenGenerator>();
        _uowMock = new Mock<IUnitOfWork>();
        _uowMock.Setup(x => x.Users.AddAsync(It.IsAny<ChatUser>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserAlreadyExists()
    {
        _userManagerMock
            .Setup(x => x.FindByNameAsync("user"))
            .ReturnsAsync(new ApplicationUser());

        var handler = new RegisterUserCommandHandler(
            _userManagerMock.Object,
            _jwtMock.Object,
            _uowMock.Object
        );

        var request = new RegisterUserCommand("user", "pass");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenIdentityCreationFails()
    {
        _userManagerMock
            .Setup(x => x.FindByNameAsync("user"))
            .ReturnsAsync((ApplicationUser)null);

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "pass"))
            .ReturnsAsync(IdentityResult.Failed());

        var handler = new RegisterUserCommandHandler(
            _userManagerMock.Object, _jwtMock.Object, _uowMock.Object);

        var request = new RegisterUserCommand("user", "pass");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnAuthResponse_WhenRegistrationSucceeds()
    {
        var user = new ApplicationUser { UserName = "user" };

        _userManagerMock.Setup(x => x.FindByNameAsync("user"))
            .ReturnsAsync((ApplicationUser)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "pass"))
            .ReturnsAsync(IdentityResult.Success);

        _jwtMock.Setup(x => x.GenerateToken(It.IsAny<ApplicationUser>()))
            .Returns("fake-jwt");

        var handler = new RegisterUserCommandHandler(
            _userManagerMock.Object, _jwtMock.Object, _uowMock.Object);

        var request = new RegisterUserCommand("user", "pass");

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(user.Id, result.Id);
        Assert.Equal("user", result.UserName);
        Assert.Equal("fake-jwt", result.Token);

        _uowMock.Verify(x => x.Users.AddAsync(It.IsAny<ChatUser>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
