using Jobsity.FinancialChat.Application.Auth.Dtos;

using MediatR;

namespace Jobsity.FinancialChat.Application.Auth.Commands.LoginUser
{
    public sealed record LoginUserRequest : IRequest<AuthResponse>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

}
