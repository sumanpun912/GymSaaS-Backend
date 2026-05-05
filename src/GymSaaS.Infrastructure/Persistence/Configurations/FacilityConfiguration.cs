using GymSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSaaS.Infrastructure.Persistence.Configurations;

internal sealed class FacilityConfiguration : IEntityTypeConfiguration<Facility>
{
    public void Configure(EntityTypeBuilder<Facility> builder)
    {
        builder.ToTable("facilities");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Name).HasMaxLength(200).IsRequired();
        builder.Property(f => f.AddressLine).HasMaxLength(400);
        builder.Property(f => f.CreatedAtUtc).IsRequired();
        builder.HasIndex(f => new { f.TenantId, f.Name });
        builder.HasOne(f => f.Tenant)
            .WithMany()
            .HasForeignKey(f => f.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
