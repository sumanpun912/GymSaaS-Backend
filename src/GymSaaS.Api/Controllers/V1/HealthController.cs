using GymSaaS.Api.Common.Http;
using GymSaaS.Application.Features.Health.Demo;
using GymSaaS.Application.Features.Health.Ping;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymSaaS.Api.Controllers.V1;

[ApiController]
[Route("api/v1")]
[Tags("Health & bootstrap")]
public sealed class HealthController(IMediator mediator) : ControllerBase
{
    [HttpGet("health/live")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Live() => Ok(new { status = "ok", utc = DateTime.UtcNow });

    /// <summary>Ping API + MediatR + ErrorOr pipeline.</summary>
    [HttpGet("ping")]
    public async Task<IActionResult> Ping(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new PingQuery(), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Sample <c>Error.NotFound</c> → HTTP 404 ProblemDetails (no exceptions).</summary>
    [HttpGet("demo/not-found")]
    public async Task<IActionResult> NotFoundDemo(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new NotFoundDemoQuery(), cancellationToken);
        return result.ToActionResult();
    }
}
