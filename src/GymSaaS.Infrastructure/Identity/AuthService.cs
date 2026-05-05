using ErrorOr;
using GymSaaS.Application.Abstractions.Auth;
using GymSaaS.Application.Features.Auth;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Enums;
using GymSaaS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymSaaS.Infrastructure.Identity;

internal sealed class AuthService(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    JwtTokenService jwtTokenService,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<ErrorOr<AuthResponse>> RegisterTenantAsync(
        string tenantSlug,
        string displayName,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var slug = tenantSlug.Trim().ToLowerInvariant();
        var existingTenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
        if (existingTenant is not null)
        {
            return Error.Conflict("Tenant.SlugTaken", "That tenant slug is already in use.");
        }

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return Error.Conflict("User.EmailTaken", "An account with this email already exists.");
        }

        try
        {
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Slug = slug,
                DisplayName = displayName.Trim(),
                CreatedAtUtc = DateTimeOffset.UtcNow
            };

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email.Trim(),
                Email = email.Trim(),
                EmailConfirmed = true
            };

            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                var msg = string.Join("; ", create.Errors.Select(e => e.Description));
                return Error.Validation("User.CreateFailed", msg);
            }

            db.Tenants.Add(tenant);
            db.TenantMemberships.Add(new TenantMembership
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                UserId = user.Id,
                Role = TenantRole.Owner,
                CreatedAtUtc = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync(cancellationToken);

            var (token, exp) = jwtTokenService.CreateAccessToken(
                user.Id,
                user.Email!,
                tenant.Id,
                tenant.Slug,
                TenantRole.Owner);

            return new AuthResponse(
                token,
                exp,
                tenant.Id,
                tenant.Slug,
                tenant.DisplayName,
                user.Id,
                user.Email!,
                TenantRole.Owner.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Register tenant failed");
            return Error.Failure("Tenant.RegisterFailed", "Could not complete registration.");
        }
    }

    public async Task<ErrorOr<AuthResponse>> LoginAsync(
        string email,
        string password,
        string tenantSlug,
        CancellationToken cancellationToken)
    {
        var slug = tenantSlug.Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user is null)
        {
            return Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            return Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");
        }

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
        if (tenant is null)
        {
            return Error.NotFound("Tenant.NotFound", "Unknown tenant slug.");
        }

        var membership = await db.TenantMemberships.AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == user.Id && m.TenantId == tenant.Id, cancellationToken);

        if (membership is null)
        {
            return Error.Forbidden("Tenant.NotMember", "This user is not a member of that gym.");
        }

        var (token, exp) = jwtTokenService.CreateAccessToken(
            user.Id,
            user.Email!,
            tenant.Id,
            tenant.Slug,
            membership.Role);

        return new AuthResponse(
            token,
            exp,
            tenant.Id,
            tenant.Slug,
            tenant.DisplayName,
            user.Id,
            user.Email!,
            membership.Role.ToString());
    }
}
