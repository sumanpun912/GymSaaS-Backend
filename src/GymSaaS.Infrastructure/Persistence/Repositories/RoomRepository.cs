using GymSaaS.Application.Abstractions.Persistence;
using GymSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSaaS.Infrastructure.Persistence.Repositories;

internal sealed class RoomRepository(ApplicationDbContext db) : IRoomRepository
{
    public async Task<IReadOnlyList<Room>> ListAsync(Guid tenantId, CancellationToken cancellationToken) =>
        await db.Rooms.AsNoTracking()
            .Include(r => r.Facility)
            .Where(r => r.TenantId == tenantId)
            .OrderBy(r => r.Facility!.Name)
            .ThenBy(r => r.Name)
            .ToListAsync(cancellationToken);

    public async Task<Room?> GetAsync(Guid tenantId, Guid id, CancellationToken cancellationToken) =>
        await db.Rooms.Include(r => r.Facility)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id, cancellationToken);

    public Task<bool> FacilityBelongsToTenantAsync(Guid tenantId, Guid facilityId, CancellationToken cancellationToken) =>
        db.Facilities.AnyAsync(f => f.TenantId == tenantId && f.Id == facilityId, cancellationToken);

    public async Task AddAsync(Room room, CancellationToken cancellationToken) =>
        await db.Rooms.AddAsync(room, cancellationToken);

    public void Remove(Room room) => db.Rooms.Remove(room);
}
