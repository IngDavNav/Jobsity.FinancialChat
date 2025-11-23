using Jobsity.FinancialChat.Application.Common.Identity;

namespace Jobsity.FinancialChat.Application.Abstractions.Auth
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(ApplicationUser user);
    }
}
