using Jobsity.FinancialChat.Application.Abstractions;
using Jobsity.FinancialChat.Application.Abstractions.Persistence;

namespace Jobsity.FinancialChat.Infrastructure.Persistance
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly ChatDbContext _dbContext;
        private readonly IChatMessageRepository _messageRepository;
        private readonly IChatRoomRepository _roomRepository;
        private readonly IChatUserRepository _userRepository;

        public UnitOfWork(IChatMessageRepository messageRepository, IChatRoomRepository roomRepository, IChatUserRepository userRepository, ChatDbContext dbContext)
        {
            _messageRepository = messageRepository;
            _roomRepository = roomRepository;
            _userRepository = userRepository;
            _dbContext = dbContext;
        }

        public IChatMessageRepository Messages => _messageRepository;
        public IChatRoomRepository Rooms => _roomRepository;
        public IChatUserRepository Users => _userRepository;

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            if (_dbContext is null)
            {
                throw new InvalidOperationException("DbContext is not initialized.");
            }

            return _dbContext.SaveChangesAsync(ct);
        }
    }
}