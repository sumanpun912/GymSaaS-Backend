using GymSaaS.Api.Common.Http;
using GymSaaS.Application.Features.Members;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSaaS.Api.Controllers.V1;

[ApiController]
[Route("api/v1/members")]
[Authorize]
public sealed class MembersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        (await mediator.Send(new ListMembersQuery(), cancellationToken)).ToActionResult();

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        (await mediator.Send(new GetMemberQuery(id), cancellationToken)).ToActionResult();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMemberBody body, CancellationToken cancellationToken) =>
        (await mediator.Send(new CreateMemberCommand(body.FullName, body.Email, body.Phone), cancellationToken))
            .ToActionResult();

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateMemberBody body,
        CancellationToken cancellationToken) =>
        (await mediator.Send(new UpdateMemberCommand(id, body.FullName, body.Email, body.Phone), cancellationToken))
            .ToActionResult();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        (await mediator.Send(new DeleteMemberCommand(id), cancellationToken)).ToActionResult();
}

public sealed record CreateMemberBody(string FullName, string Email, string? Phone);

public sealed record UpdateMemberBody(string FullName, string Email, string? Phone);
