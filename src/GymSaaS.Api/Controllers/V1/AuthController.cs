using GymSaaS.Api.Common.Http;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSaaS.Api.Controllers.V1;

[ApiController]
[Route("api/v1/auth")]
[AllowAnonymous]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest body, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RegisterTenantCommand(body.TenantSlug, body.DisplayName, body.Email, body.Password),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest body, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new LoginCommand(body.Email, body.Password, body.TenantSlug),
            cancellationToken);

        return result.ToActionResult();
    }
}

public sealed record RegisterRequest(string TenantSlug, string DisplayName, string Email, string Password);

public sealed record LoginRequest(string Email, string Password, string TenantSlug);
