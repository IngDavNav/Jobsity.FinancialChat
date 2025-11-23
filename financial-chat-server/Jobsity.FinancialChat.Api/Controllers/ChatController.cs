using Jobsity.FinancialChat.Application.Messages.Commands.SendMessage;
using Jobsity.FinancialChat.Application.Messages.Dtos;
using Jobsity.FinancialChat.Application.Messages.Queries;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jobsity.FinancialChat.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Sends a new chat message to a room.
    /// </summary>
    /// <param name="command">Message payload.</param>
    /// <returns>The created message, or null if it was a bot command.</returns>
    /// <response code="200">Message processed successfully.</response>
    /// <response code="400">Invalid payload.</response>
    [HttpPost("send")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChatMessageDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatMessageDto?>> Send(
        [FromBody] SendMessageCommand command,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command, ct).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Gets the chat history for a specific room.
    /// </summary>
    /// <param name="roomId">Room identifier.</param>
    /// <returns>Messages ordered by timestamp ascending.</returns>
    /// <response code="200">History returned successfully.</response>
    [HttpGet("history/{roomId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ChatMessageDto>))]
    public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetHistory(
        [FromRoute] Guid roomId,
        CancellationToken ct)
    {
        var query = new GetMessagesByRoomQuery
        {
            RoomId = roomId
        };

        var result = await _mediator.Send(query, ct).ConfigureAwait(false);
        return Ok(result);
    }
}
