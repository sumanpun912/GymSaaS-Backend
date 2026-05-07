using GymSaaS.Api.Common.Http;
using GymSaaS.Application.Features.Rooms;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSaaS.Api.Controllers.V1;

[ApiController]
[Route("api/v1/rooms")]
[Authorize]
public sealed class RoomsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        (await mediator.Send(new ListRoomsQuery(), cancellationToken)).ToActionResult();

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        (await mediator.Send(new GetRoomQuery(id), cancellationToken)).ToActionResult();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoomBody body, CancellationToken cancellationToken) =>
        (await mediator.Send(new CreateRoomCommand(body.FacilityId, body.Name, body.Capacity), cancellationToken))
            .ToActionResult();

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateRoomBody body,
        CancellationToken cancellationToken) =>
        (await mediator.Send(new UpdateRoomCommand(id, body.Name, body.Capacity), cancellationToken)).ToActionResult();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        (await mediator.Send(new DeleteRoomCommand(id), cancellationToken)).ToActionResult();
}

public sealed record CreateRoomBody(Guid FacilityId, string Name, int Capacity);

public sealed record UpdateRoomBody(string Name, int Capacity);
