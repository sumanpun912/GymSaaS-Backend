using GymSaaS.Application.Abstractions.Persistence;
using GymSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSaaS.Infrastructure.Persistence.Repositories;

internal sealed class MemberRepository(ApplicationDbContext db) : IMemberRepository
{
    public async Task<IReadOnlyList<Member>> ListAsync(Guid tenantId, CancellationToken cancellationToken) =>
        await db.Members.AsNoTracking()
            .Where(m => m.TenantId == tenantId)
            .OrderBy(m => m.FullName)
            .ToListAsync(cancellationToken);

    public async Task<Member?> GetAsync(Guid tenantId, Guid id, CancellationToken cancellationToken) =>
        await db.Members.FirstOrDefaultAsync(m => m.TenantId == tenantId && m.Id == id, cancellationToken);

    public Task<bool> EmailExistsInTenantAsync(
        Guid tenantId,
        string email,
        Guid? excludeMemberId,
        CancellationToken cancellationToken)
    {
        var q = db.Members.Where(m => m.TenantId == tenantId && m.Email == email);
        if (excludeMemberId is { } ex)
        {
            q = q.Where(m => m.Id != ex);
        }

        return q.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Member member, CancellationToken cancellationToken) =>
        await db.Members.AddAsync(member, cancellationToken);

    public void Remove(Member member) => db.Members.Remove(member);
}
