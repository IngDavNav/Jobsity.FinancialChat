using Jobsity.FinancialChat.Application.ChatRooms;

using MediatR;

namespace Jobsity.FinancialChat.Application.ChatRooms.Commands.CreateChatRoom;

public sealed record CreateRoomCommand(string Name)
    : IRequest<ChatRoomDto>;
