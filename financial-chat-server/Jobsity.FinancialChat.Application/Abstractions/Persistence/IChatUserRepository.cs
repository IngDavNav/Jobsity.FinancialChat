using Jobsity.FinancialChat.Domain.Models;

namespace Jobsity.FinancialChat.Application.Abstractions.Persistence
{
    public interface IChatUserRepository
    {
        Task<ChatUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<ChatUser?> GetByUserNameAsync(string userName, CancellationToken ct = default);
        Task AddAsync(ChatUser user, CancellationToken ct = default);
    }
}
