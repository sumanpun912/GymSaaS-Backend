using ErrorOr;
using MediatR;

namespace GymSaaS.Application.Features.Health.Demo;

public sealed record DemoEnvelope(string Message);

public sealed record NotFoundDemoQuery : IRequest<ErrorOr<DemoEnvelope>>;

public sealed class NotFoundDemoQueryHandler : IRequestHandler<NotFoundDemoQuery, ErrorOr<DemoEnvelope>>
{
    public Task<ErrorOr<DemoEnvelope>> Handle(NotFoundDemoQuery request, CancellationToken cancellationToken)
    {
        ErrorOr<DemoEnvelope> result = Error.NotFound("demo.not_found", "The demo was not found.");

        return Task.FromResult(result);
    }
}