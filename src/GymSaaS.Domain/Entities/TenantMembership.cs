using GymSaaS.Domain.Enums;

namespace GymSaaS.Domain.Entities;

/// Links an identity user to a tenant with a gym-level role.
public sealed class TenantMembership
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public Guid UserId { get; set; }
    public TenantRole Role { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
