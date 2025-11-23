using Jobsity.FinancialChat.Application.Abstractions;
using Jobsity.FinancialChat.Application.Abstractions.Persistence;
using Jobsity.FinancialChat.Domain.Models;

using Microsoft.EntityFrameworkCore;

namespace Jobsity.FinancialChat.Infrastructure.Persistance.Repositories
{
    internal class ChatMessageRepository : IChatMessageRepository
    {
        private readonly ChatDbContext _dbContext;

        public ChatMessageRepository(ChatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(ChatMessage message, CancellationToken ct = default)
        {
            await _dbContext.ChatMessages.AddAsync(message, ct).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<ChatMessage>> GetLastMessagesAsync(Guid roomId, int take, CancellationToken ct = default)
        {
            var messages = await _dbContext.ChatMessages
                .Include(m => m.User)
                .AsNoTracking()
                .Where(m => m.RoomId == roomId)
                .OrderBy(m => m.TimeStamp)
                .Take(take)
                .ToListAsync();

            return messages;
        }
    }
}