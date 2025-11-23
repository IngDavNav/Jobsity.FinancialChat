using Jobsity.FinancialChat.Application.Abstractions;
using Jobsity.FinancialChat.Application.Abstractions.Auth;
using Jobsity.FinancialChat.Application.Auth.Dtos;
using Jobsity.FinancialChat.Application.Common.Identity;
using Jobsity.FinancialChat.Domain.Models;

using MediatR;

using Microsoft.AspNetCore.Identity;
namespace Jobsity.FinancialChat.Application.Auth.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler :
    IRequestHandler<RegisterUserCommand, AuthResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    public RegisterUserCommandHandler(UserManager<ApplicationUser> userManager, IJwtTokenGenerator tokenGenerator, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
        _unitOfWork = unitOfWork;
    }
    public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var userName = request.UserName?.Trim();
        if (await _userManager.FindByNameAsync(userName) is not null)
            throw new InvalidOperationException($"User with username '{userName}' already exists.");


        var identityUser = new ApplicationUser { UserName = userName };
        var result = await _userManager.CreateAsync(identityUser, request.Password).ConfigureAwait(false);
        if (!result.Succeeded)

            throw new InvalidOperationException("Unable to register user.");

        var chatUser = new ChatUser
        { Id = identityUser.Id, UserName = userName };

        await _unitOfWork.Users.AddAsync(chatUser, ct).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        var token = _tokenGenerator.GenerateToken(identityUser);
        return new AuthResponse(chatUser.Id, chatUser.UserName, token);
    }
}