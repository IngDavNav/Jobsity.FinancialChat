using Jobsity.FinancialChat.Application.Abstractions.Persistence;

namespace Jobsity.FinancialChat.Application.Abstractions;

public interface IUnitOfWork
{
    public IChatMessageRepository Messages { get; }
    public IChatRoomRepository Rooms { get; }
    public IChatUserRepository Users { get; }


    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
