using Jobsity.FinancialChat.Application.Abstractions.Persistence;
using Jobsity.FinancialChat.Domain.Models;

using Microsoft.EntityFrameworkCore;

namespace Jobsity.FinancialChat.Infrastructure.Persistance.Repositories;

internal class ChatRoomRepository : IChatRoomRepository
{
    private readonly ChatDbContext _dbContext;

    public ChatRoomRepository(ChatDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task AddAsync(ChatRoom room, CancellationToken ct = default)
    {
        await _dbContext.ChatRooms.AddAsync(room, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ChatRoom>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.ChatRooms.ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<ChatRoom?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.ChatRooms.FirstOrDefaultAsync(r => r.Id == id, ct).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await _dbContext.ChatRooms.AnyAsync(r => r.Name == name, ct).ConfigureAwait(false);
    }
}