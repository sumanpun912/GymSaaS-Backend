using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GymSaaS.Infrastructure.Persistence;

/// Used by EF Core CLI when generating migrations.
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private const string Url =
        "Host=localhost;Port=5432;Database=gymsaas;Username=postgres;Password=postgres";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(Url)
            .Options;

        return new ApplicationDbContext(options);
    }
}
