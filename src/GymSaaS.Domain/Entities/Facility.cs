namespace GymSaaS.Domain.Entities;

public sealed class Facility
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public required string Name { get; set; }
    public string? AddressLine { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
