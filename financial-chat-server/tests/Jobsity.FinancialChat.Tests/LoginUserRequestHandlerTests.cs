using System;
using System.Threading;
using System.Threading.Tasks;
using Jobsity.FinancialChat.Application.Auth.Commands.LoginUser;
using Jobsity.FinancialChat.Application.Auth.Dtos;
using Jobsity.FinancialChat.Application.Common.Identity;
using Jobsity.FinancialChat.Application.Abstractions.Auth;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace Jobsity.FinancialChat.Tests.Application;

public class LoginUserRequestHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IJwtTokenGenerator> _jwtMock;

    public LoginUserRequestHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null
        );

        _jwtMock = new Mock<IJwtTokenGenerator>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserDoesNotExist()
    {
        // Arrange
        _userManagerMock
            .Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null);

        var handler = new LoginUserRequestHandler(_userManagerMock.Object, _jwtMock.Object);

        var request = new LoginUserRequest
        {
            UserName = "user",
            Password = "pass"
        };

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenPasswordInvalid()
    {
        var user = new ApplicationUser { UserName = "user" };

        _userManagerMock
            .Setup(x => x.FindByNameAsync("user"))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, "pass"))
            .ReturnsAsync(false);

        var handler = new LoginUserRequestHandler(_userManagerMock.Object, _jwtMock.Object);

        var request = new LoginUserRequest
        {
            UserName = "user",
            Password = "pass"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(request, CancellationToken.None));
    }

    [Fact]
        public async Task Handle_ShouldReturnAuthResponse_WhenCredentialsAreValid()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "user"
        };

        _userManagerMock.Setup(x => x.FindByNameAsync("user"))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "pass"))
            .ReturnsAsync(true);

        _jwtMock.Setup(x => x.GenerateToken(user))
            .Returns("fake-jwt");

        var handler = new LoginUserRequestHandler(_userManagerMock.Object, _jwtMock.Object);

        var request = new LoginUserRequest
        {
            UserName = "user",
            Password = "pass"
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(user.Id, result.Id);
        Assert.Equal("user", result.UserName);
        Assert.Equal("fake-jwt", result.Token);
    }
}
