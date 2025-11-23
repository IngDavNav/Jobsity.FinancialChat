using Jobsity.FinancialChat.Application.Messages.Dtos;
using MediatR;

namespace Jobsity.FinancialChat.Application.Messages.Commands.SendMessage;

public sealed record SendMessageCommand(
    Guid RoomId,
    Guid UserId,
    string Text
) : IRequest<ChatMessageDto?>;
