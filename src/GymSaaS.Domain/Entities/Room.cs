namespace GymSaaS.Domain.Entities;

public sealed class Room
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public Guid FacilityId { get; set; }
    public Facility Facility { get; set; } = null!;
    public required string Name { get; set; }
    public int Capacity { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
