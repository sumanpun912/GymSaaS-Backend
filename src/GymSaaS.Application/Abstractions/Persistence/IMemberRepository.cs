using GymSaaS.Domain.Entities;

namespace GymSaaS.Application.Abstractions.Persistence;

public interface IMemberRepository
{
    Task<IReadOnlyList<Member>> ListAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<Member?> GetAsync(Guid tenantId, Guid id, CancellationToken cancellationToken);
    Task<bool> EmailExistsInTenantAsync(Guid tenantId, string email, Guid? excludeMemberId, CancellationToken cancellationToken);
    Task AddAsync(Member member, CancellationToken cancellationToken);
    void Remove(Member member);
}
