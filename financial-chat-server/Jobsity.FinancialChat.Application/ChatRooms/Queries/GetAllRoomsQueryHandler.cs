using AutoMapper;

using Jobsity.FinancialChat.Application.Abstractions;
using Jobsity.FinancialChat.Application.ChatRooms;

using MediatR;

namespace Jobsity.FinancialChat.Application.ChatRooms.Queries;

public sealed class GetAllRoomsQueryHandler
    : IRequestHandler<GetAllRoomsQuery, IReadOnlyList<ChatRoomDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllRoomsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ChatRoomDto>> Handle(
        GetAllRoomsQuery request,
        CancellationToken cancellationToken)
    {
        var rooms = await _unitOfWork.Rooms.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var dto = _mapper.Map<IReadOnlyList<ChatRoomDto>>(rooms);
        return dto;
    }
}
