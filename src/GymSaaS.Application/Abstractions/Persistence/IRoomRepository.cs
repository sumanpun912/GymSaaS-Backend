using GymSaaS.Domain.Entities;

namespace GymSaaS.Application.Abstractions.Persistence;

public interface IRoomRepository
{
    Task<IReadOnlyList<Room>> ListAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<Room?> GetAsync(Guid tenantId, Guid id, CancellationToken cancellationToken);
    Task<bool> FacilityBelongsToTenantAsync(Guid tenantId, Guid facilityId, CancellationToken cancellationToken);
    Task AddAsync(Room room, CancellationToken cancellationToken);
    void Remove(Room room);
}
