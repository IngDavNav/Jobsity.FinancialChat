using Jobsity.FinancialChat.Application.Auth.Dtos;

using MediatR;

namespace Jobsity.FinancialChat.Application.Auth.Commands.RegisterUser
{
    public sealed record RegisterUserCommand(string UserName, string Password)
        : IRequest<AuthResponse>;
}
