using GymSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSaaS.Infrastructure.Persistence.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Slug).HasMaxLength(160).IsRequired();
        builder.HasIndex(t => t.Slug).IsUnique();
        builder.Property(t => t.DisplayName).HasMaxLength(260).IsRequired();
        builder.Property(t => t.CreatedAtUtc).IsRequired();
    }
}
