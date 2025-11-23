using Jobsity.FinancialChat.Application.ChatRooms;
using Jobsity.FinancialChat.Application.ChatRooms.Commands.CreateChatRoom;
using Jobsity.FinancialChat.Application.ChatRooms.Queries;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace Jobsity.FinancialChat.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public sealed class RoomsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoomsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns all chat rooms.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ChatRoomDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllRoomsQuery(), ct).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new chat room.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ChatRoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRoomCommand request,
        CancellationToken ct)
    {
        var command = new CreateRoomCommand(request.Name);
        var created = await _mediator.Send(command, ct).ConfigureAwait(false);

        return Ok(created);
    }
}