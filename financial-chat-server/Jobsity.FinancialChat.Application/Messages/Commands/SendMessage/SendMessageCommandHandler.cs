using AutoMapper;

using Jobsity.FinancialChat.Application.Abstractions;
using Jobsity.FinancialChat.Application.Abstractions.Messaging;
using Jobsity.FinancialChat.Application.Abstractions.Services;
using Jobsity.FinancialChat.Application.Bot.Contracts;
using Jobsity.FinancialChat.Application.Messages.Dtos;
using Jobsity.FinancialChat.Domain.Models;

using MediatR;


namespace Jobsity.FinancialChat.Application.Messages.Commands.SendMessage;
public sealed class SendMessageCommandHandler
    : IRequestHandler<SendMessageCommand, ChatMessageDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatNotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly IStockCommandBus _stockCommandBus;

    public SendMessageCommandHandler(IUnitOfWork unitOfWork, IChatNotificationService notificationService, IMapper mapper, IStockCommandBus stockCommandBus)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _mapper = mapper;
        _stockCommandBus = stockCommandBus;
    }


    public async Task<ChatMessageDto?> Handle(
        SendMessageCommand request,
        CancellationToken ct)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, ct).ConfigureAwait(false)
                   ?? throw new InvalidOperationException("User not found.");

        var room = await _unitOfWork.Rooms.GetByIdAsync(request.RoomId, ct).ConfigureAwait(false)
                   ?? throw new InvalidOperationException("Chat room not found.");

        if (EnqueueBotStockCommand.TryParse(request.RoomId, request.Text, out var stockCmd))
        {
            await _stockCommandBus.EnqueueAsync(stockCmd!, ct).ConfigureAwait(false);

            return null;
        }

        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            ChatRoom = room,
            UserId = user.Id,
            User = user,
            Text = request.Text,
            TimeStamp = DateTime.UtcNow
        };

        await _unitOfWork.Messages.AddAsync(message, ct).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        var messageDto = _mapper.Map<ChatMessageDto>(message);
        await _notificationService.NotifyMessageAsync(messageDto, ct).ConfigureAwait(false);

        return messageDto;
    }
}
