namespace GymSaaS.Domain.Entities;
public sealed class Tenant
{
    public Guid Id { get; set; }
    public required string Slug { get; set; }
    public required string DisplayName { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
