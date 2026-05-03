using ErrorOr;
using MediatR;

namespace GymSaaS.Application.Features.Health.Ping;

public sealed record PingQuery : IRequest<ErrorOr<PingResponse>>;

public sealed record PingResponse(string MessageUtc);

public sealed class GetPingHandler : IRequestHandler<PingQuery, ErrorOr<PingResponse>>
{
    public Task<ErrorOr<PingResponse>> Handle(PingQuery request, CancellationToken cancellationToken)
    {
        var dto = new PingResponse($"ping @ {DateTime.UtcNow:o}");
        return Task.FromResult<ErrorOr<PingResponse>>(dto);
    }
}