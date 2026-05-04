using GymSaaS.Domain.Entities;

namespace GymSaaS.Application.Abstractions.Persistence;

public interface IFacilityRepository
{
    Task<IReadOnlyList<Facility>> ListAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<Facility?> GetAsync(Guid tenantId, Guid id, CancellationToken cancellationToken);
    Task AddAsync(Facility facility, CancellationToken cancellationToken);
    void Remove(Facility facility);
}
