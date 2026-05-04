using ErrorOr;
using GymSaaS.Application.Features.Auth;

namespace GymSaaS.Application.Abstractions.Auth;

public interface IAuthService
{
    Task<ErrorOr<AuthResponse>> RegisterTenantAsync(
        string tenantSlug,
        string displayName,
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<ErrorOr<AuthResponse>> LoginAsync(
        string email,
        string password,
        string tenantSlug,
        CancellationToken cancellationToken);
}
