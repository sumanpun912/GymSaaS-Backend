namespace GymSaaS.Application.Features.Auth;

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    Guid TenantId,
    string TenantSlug,
    string TenantDisplayName,
    Guid UserId,
    string Email,
    string Role);
