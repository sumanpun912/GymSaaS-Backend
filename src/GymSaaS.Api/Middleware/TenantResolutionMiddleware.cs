using System.Security.Claims;
using GymSaaS.Application.Abstractions;
using GymSaaS.Application.Common.Constants;
using GymSaaS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymSaaS.Api.Middleware;

/// For authenticated API calls, requires <see cref="AuthConstants.TenantSlugHeader"/> and checks it matches the JWT tenant.
public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ApplicationDbContext db,
        ICurrentTenant currentTenant)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/api/v1/auth", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        if (string.Equals(path, "/api/v1/ping", StringComparison.OrdinalIgnoreCase)
            || string.Equals(path, "/api/v1/health/live", StringComparison.OrdinalIgnoreCase)
            || string.Equals(path, "/api/v1/demo/not-found", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(AuthConstants.TenantSlugHeader, out var slugHeader)
            || string.IsNullOrWhiteSpace(slugHeader))
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Missing tenant",
                $"Authenticated requests must include header '{AuthConstants.TenantSlugHeader}'.");
            return;
        }

        var slug = slugHeader.ToString().Trim().ToLowerInvariant();
        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == slug, context.RequestAborted);
        if (tenant is null)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Unknown tenant",
                "The tenant slug in the header does not match a registered gym.");
            return;
        }

        var claimTenantId = context.User.FindFirstValue(AuthConstants.TenantIdClaim);
        if (claimTenantId is null || !Guid.TryParse(claimTenantId, out var jwtTenantId) || jwtTenantId != tenant.Id)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status403Forbidden,
                "Tenant mismatch",
                "The header tenant does not match the tenant encoded in your access token.");
            return;
        }

        currentTenant.SetTenant(tenant.Id, tenant.Slug);
        await next(context);
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int status,
        string title,
        string detail)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = "about:blank"
        };
        await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
    }
}
