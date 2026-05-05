using GymSaaS.Application.Abstractions.Persistence;
using GymSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSaaS.Infrastructure.Persistence.Repositories;

internal sealed class FacilityRepository(ApplicationDbContext db) : IFacilityRepository
{
    public async Task<IReadOnlyList<Facility>> ListAsync(Guid tenantId, CancellationToken cancellationToken) =>
        await db.Facilities.AsNoTracking()
            .Where(f => f.TenantId == tenantId)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);

    public async Task<Facility?> GetAsync(Guid tenantId, Guid id, CancellationToken cancellationToken) =>
        await db.Facilities.FirstOrDefaultAsync(f => f.TenantId == tenantId && f.Id == id, cancellationToken);

    public async Task AddAsync(Facility facility, CancellationToken cancellationToken) =>
        await db.Facilities.AddAsync(facility, cancellationToken);

    public void Remove(Facility facility) => db.Facilities.Remove(facility);
}
