using AutoMapper;

using Jobsity.FinancialChat.Application.Abstractions;
using Jobsity.FinancialChat.Application.Messages.Dtos;

using MediatR;

namespace Jobsity.FinancialChat.Application.Messages.Queries;

public sealed class GetMessagesByRoomQueryHandler
    : IRequestHandler<GetMessagesByRoomQuery, IEnumerable<ChatMessageDto>>
{
    private const int MessageLimit = 50;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMessagesByRoomQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ChatMessageDto>> Handle(
        GetMessagesByRoomQuery request,
        CancellationToken cancellationToken)
    {
        var messages = await _unitOfWork.Messages
            .GetLastMessagesAsync(request.RoomId, MessageLimit, cancellationToken).ConfigureAwait(false);

        return _mapper.Map<IEnumerable<ChatMessageDto>>(messages);
    }
}
