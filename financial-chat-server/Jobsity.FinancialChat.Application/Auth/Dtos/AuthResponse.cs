namespace Jobsity.FinancialChat.Application.Auth.Dtos
{
    public sealed record AuthResponse(
        Guid Id,
        string UserName,
        string Token);
}
