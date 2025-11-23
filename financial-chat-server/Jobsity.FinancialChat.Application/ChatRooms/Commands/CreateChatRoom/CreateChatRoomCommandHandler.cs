using AutoMapper;

using Jobsity.FinancialChat.Application.Abstractions;
using Jobsity.FinancialChat.Application.ChatRooms;
using Jobsity.FinancialChat.Domain.Models;

using MediatR;

namespace Jobsity.FinancialChat.Application.ChatRooms.Commands.CreateChatRoom;

public sealed class CreateRoomCommandHandler
    : IRequestHandler<CreateRoomCommand, ChatRoomDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateRoomCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ChatRoomDto> Handle(
        CreateRoomCommand request,
        CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room name is required.");

        var exists = await _unitOfWork.Rooms
            .ExistsByNameAsync(name, cancellationToken);

        if (exists)
            throw new InvalidOperationException($"Room '{name}' already exists.");

        var room = new ChatRoom
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        await _unitOfWork.Rooms.AddAsync(room, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return _mapper.Map<ChatRoomDto>(room);
    }
}
