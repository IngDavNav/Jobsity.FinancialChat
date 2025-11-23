using Jobsity.FinancialChat.Application.Abstractions.Auth;
using Jobsity.FinancialChat.Application.Auth.Commands.LoginUser;
using Jobsity.FinancialChat.Application.Auth.Dtos;
using Jobsity.FinancialChat.Application.Common.Identity;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace Jobsity.FinancialChat.Application.Auth.Commands.LoginUser;
public sealed class LoginUserRequestHandler : IRequestHandler<LoginUserRequest, AuthResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _tokenGenerator;
    public LoginUserRequestHandler(UserManager<ApplicationUser> userManager, IJwtTokenGenerator tokenGenerator)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
    }
    public async Task<AuthResponse> Handle(LoginUserRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByNameAsync(request.UserName).ConfigureAwait(false);
        if (user is null)
            throw new InvalidOperationException("Invalid credentials.");

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password).ConfigureAwait(false); if (!validPassword)
            throw new InvalidOperationException("Invalid credentials.");
        var token = _tokenGenerator.GenerateToken(user);
        return new AuthResponse(user.Id, user.UserName!, token);
    }
}