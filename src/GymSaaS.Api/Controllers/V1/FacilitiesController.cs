using GymSaaS.Api.Common.Http;
using GymSaaS.Application.Features.Facilities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSaaS.Api.Controllers.V1;

[ApiController]
[Route("api/v1/facilities")]
[Authorize]
public sealed class FacilitiesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        (await mediator.Send(new ListFacilitiesQuery(), cancellationToken)).ToActionResult();

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        (await mediator.Send(new GetFacilityQuery(id), cancellationToken)).ToActionResult();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFacilityBody body, CancellationToken cancellationToken) =>
        (await mediator.Send(new CreateFacilityCommand(body.Name, body.AddressLine), cancellationToken)).ToActionResult();

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateFacilityBody body,
        CancellationToken cancellationToken) =>
        (await mediator.Send(new UpdateFacilityCommand(id, body.Name, body.AddressLine), cancellationToken))
            .ToActionResult();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        (await mediator.Send(new DeleteFacilityCommand(id), cancellationToken)).ToActionResult();
}

public sealed record CreateFacilityBody(string Name, string? AddressLine);

public sealed record UpdateFacilityBody(string Name, string? AddressLine);
