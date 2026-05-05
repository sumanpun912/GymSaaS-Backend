using GymSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSaaS.Infrastructure.Persistence.Configurations;

internal sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("members");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.FullName).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Email).HasMaxLength(256).IsRequired();
        builder.Property(m => m.Phone).HasMaxLength(40);
        builder.Property(m => m.CreatedAtUtc).IsRequired();
        builder.HasIndex(m => new { m.TenantId, m.Email }).IsUnique();
        builder.HasOne(m => m.Tenant)
            .WithMany()
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
