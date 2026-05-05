using GymSaaS.Application.Abstractions;

namespace GymSaaS.Infrastructure.Tenants;

public sealed class CurrentTenant : ICurrentTenant
{
    public Guid? TenantId { get; private set; }
    public string? Slug { get; private set; }

    public void SetTenant(Guid tenantId, string slug)
    {
        TenantId = tenantId;
        Slug = slug;
    }
}
