using Jobsity.FinancialChat.Domain.Models;

namespace Jobsity.FinancialChat.Application.Abstractions.Persistence;

public interface IChatMessageRepository
{
    Task<IReadOnlyList<ChatMessage>> GetLastMessagesAsync(
        Guid roomId,
        int take,
        CancellationToken ct = default);

    Task AddAsync(ChatMessage message, CancellationToken ct = default);
}
