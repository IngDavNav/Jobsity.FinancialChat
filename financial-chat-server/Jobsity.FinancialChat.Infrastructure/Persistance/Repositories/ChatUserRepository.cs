using Jobsity.FinancialChat.Application.Abstractions.Persistence;
using Jobsity.FinancialChat.Domain.Models;

using Microsoft.EntityFrameworkCore;

namespace Jobsity.FinancialChat.Infrastructure.Persistance.Repositories
{
    internal class ChatUserRepository : IChatUserRepository
    {
        private readonly ChatDbContext _dbContext;

        public ChatUserRepository(ChatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(ChatUser user, CancellationToken ct = default)
        {
            await _dbContext.AddAsync(user, ct).ConfigureAwait(false);
        }

        public async Task<ChatUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContext.ChatUsers.FirstOrDefaultAsync(u => u.Id == id, ct).ConfigureAwait(false);
        }

        public async Task<ChatUser?> GetByUserNameAsync(string userName, CancellationToken ct = default)
        {
            return await _dbContext.ChatUsers.FirstOrDefaultAsync(u => u.UserName == userName, ct).ConfigureAwait(false);
        }
    }
}