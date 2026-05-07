using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Enums;
using GymSaaS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymSaaS.Infrastructure.Persistence.Seeding;

public sealed class DbSeeder(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    ILogger<DbSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        const string slug = "demo-gym";
        const string displayName = "Demo Gym";
        const string ownerEmail = "owner@example.com";
        const string ownerPassword = "Password1!";

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
        if (tenant is null)
        {
            tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Slug = slug,
                DisplayName = displayName,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seed: created tenant {Slug}", slug);
        }

        var owner = await userManager.FindByEmailAsync(ownerEmail);
        if (owner is null)
        {
            owner = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = ownerEmail,
                Email = ownerEmail,
                EmailConfirmed = true
            };

            var create = await userManager.CreateAsync(owner, ownerPassword);
            if (!create.Succeeded)
            {
                var msg = string.Join("; ", create.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Seed user create failed: {msg}");
            }
            logger.LogInformation("Seed: created owner user {Email}", ownerEmail);
        }

        var membershipExists = await db.TenantMemberships.AnyAsync(
            m => m.TenantId == tenant.Id && m.UserId == owner.Id,
            cancellationToken);

        if (!membershipExists)
        {
            db.TenantMemberships.Add(new TenantMembership
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                UserId = owner.Id,
                Role = TenantRole.Owner,
                CreatedAtUtc = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seed: added owner membership");
        }

        var facility = await db.Facilities.FirstOrDefaultAsync(
            f => f.TenantId == tenant.Id && f.Name == "Main Location",
            cancellationToken);

        if (facility is null)
        {
            facility = new Facility
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Name = "Main Location",
                AddressLine = "123 Main St",
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            db.Facilities.Add(facility);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seed: created facility");
        }

        var roomExists = await db.Rooms.AnyAsync(
            r => r.TenantId == tenant.Id && r.FacilityId == facility.Id && r.Name == "Studio A",
            cancellationToken);
        if (!roomExists)
        {
            db.Rooms.Add(new Room
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                FacilityId = facility.Id,
                Name = "Studio A",
                Capacity = 20,
                CreatedAtUtc = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seed: created room");
        }

        var memberExists = await db.Members.AnyAsync(
            m => m.TenantId == tenant.Id && m.Email == "member1@example.com",
            cancellationToken);
        if (!memberExists)
        {
            db.Members.Add(new Member
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                FullName = "Member One",
                Email = "member1@example.com",
                Phone = "555-0100",
                CreatedAtUtc = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seed: created member");
        }
    }
}

