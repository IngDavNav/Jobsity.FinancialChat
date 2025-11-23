using Jobsity.FinancialChat.Application.Messages.Dtos;

using MediatR;

namespace Jobsity.FinancialChat.Application.Messages.Queries
{
    public class GetMessagesByRoomQuery : IRequest<IEnumerable<ChatMessageDto>>
    {
        public Guid RoomId { get; set; }
    }
}
