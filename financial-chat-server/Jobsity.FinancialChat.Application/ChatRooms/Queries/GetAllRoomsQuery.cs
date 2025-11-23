using MediatR;

namespace Jobsity.FinancialChat.Application.ChatRooms.Queries
{
    public sealed record GetAllRoomsQuery : IRequest<IReadOnlyList<ChatRoomDto>>;
}
