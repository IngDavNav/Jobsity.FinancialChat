using Jobsity.FinancialChat.Application.Auth.Commands.LoginUser;
using Jobsity.FinancialChat.Application.Auth.Commands.RegisterUser;
using Jobsity.FinancialChat.Application.Auth.Dtos;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace Jobsity.FinancialChat.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand request, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(request, ct).ConfigureAwait(false);

            return CreatedAtAction(nameof(Register), result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(request, ct).ConfigureAwait(false);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException)
        {
            return Unauthorized(new { error = "Invalid credentials." });
        }
    }
}
