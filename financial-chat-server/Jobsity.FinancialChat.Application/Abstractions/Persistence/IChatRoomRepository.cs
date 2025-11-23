using Jobsity.FinancialChat.Domain.Models;

namespace Jobsity.FinancialChat.Application.Abstractions.Persistence;

public interface IChatRoomRepository
{
    Task<ChatRoom?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ChatRoom>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(ChatRoom room, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);

}
