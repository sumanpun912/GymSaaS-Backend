namespace GymSaaS.Application.Abstractions;

/// Per-request tenant context set by middleware after validating header + JWT.
public interface ICurrentTenant
{
    Guid? TenantId { get; }
    string? Slug { get; }
    void SetTenant(Guid tenantId, string slug);
}
