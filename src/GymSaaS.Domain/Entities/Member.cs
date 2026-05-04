namespace GymSaaS.Domain.Entities;

public sealed class Member
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
