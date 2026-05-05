using GymSaaS.Application.Abstractions;
using GymSaaS.Application.Abstractions.Auth;
using GymSaaS.Application.Abstractions.Persistence;
using GymSaaS.Infrastructure.Identity;
using GymSaaS.Infrastructure.Options;
using GymSaaS.Infrastructure.Persistence;
using GymSaaS.Infrastructure.Persistence.Repositories;
using GymSaaS.Infrastructure.Tenants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GymSaaS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("ConnectionStrings:Default is required.");

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddSingleton<JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentTenant, CurrentTenant>();
        services.AddScoped<IFacilityRepository, FacilityRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }
}
