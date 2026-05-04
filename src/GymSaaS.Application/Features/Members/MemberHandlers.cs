using ErrorOr;
using FluentValidation;
using GymSaaS.Application.Abstractions;
using GymSaaS.Application.Abstractions.Persistence;
using GymSaaS.Domain.Entities;
using MediatR;

namespace GymSaaS.Application.Features.Members;

public sealed record MemberDto(Guid Id, string FullName, string Email, string? Phone, DateTimeOffset CreatedAtUtc);

public sealed record ListMembersQuery : IRequest<ErrorOr<IReadOnlyList<MemberDto>>>;

public sealed class ListMembersQueryHandler(ICurrentTenant tenant, IMemberRepository repo)
    : IRequestHandler<ListMembersQuery, ErrorOr<IReadOnlyList<MemberDto>>>
{
    public async Task<ErrorOr<IReadOnlyList<MemberDto>>> Handle(
        ListMembersQuery request,
        CancellationToken cancellationToken)
    {
        if (tenant.TenantId is not { } tid)
        {
            return Error.Validation("Tenant.Missing", "Tenant is not resolved for this request.");
        }

        var list = await repo.ListAsync(tid, cancellationToken);
        return list.Select(m => new MemberDto(m.Id, m.FullName, m.Email, m.Phone, m.CreatedAtUtc)).ToList();
    }
}

public sealed record GetMemberQuery(Guid Id) : IRequest<ErrorOr<MemberDto>>;

public sealed class GetMemberQueryHandler(ICurrentTenant tenant, IMemberRepository repo)
    : IRequestHandler<GetMemberQuery, ErrorOr<MemberDto>>
{
    public async Task<ErrorOr<MemberDto>> Handle(GetMemberQuery request, CancellationToken cancellationToken)
    {
        if (tenant.TenantId is not { } tid)
        {
            return Error.Validation("Tenant.Missing", "Tenant is not resolved for this request.");
        }

        var m = await repo.GetAsync(tid, request.Id, cancellationToken);
        if (m is null)
        {
            return Error.NotFound("Member.NotFound", "Member was not found.");
        }

        return new MemberDto(m.Id, m.FullName, m.Email, m.Phone, m.CreatedAtUtc);
    }
}

public sealed record CreateMemberCommand(string FullName, string Email, string? Phone) : IRequest<ErrorOr<Guid>>;

public sealed class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(40);
    }
}

public sealed class CreateMemberCommandHandler(
    ICurrentTenant tenant,
    IMemberRepository repo,
    IUnitOfWork uow) : IRequestHandler<CreateMemberCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        if (tenant.TenantId is not { } tid)
        {
            return Error.Validation("Tenant.Missing", "Tenant is not resolved for this request.");
        }

        if (await repo.EmailExistsInTenantAsync(tid, request.Email.Trim(), null, cancellationToken))
        {
            return Error.Conflict("Member.EmailTaken", "A member with this email already exists in this gym.");
        }

        var entity = new Member
        {
            Id = Guid.NewGuid(),
            TenantId = tid,
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        await repo.AddAsync(entity, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}

public sealed record UpdateMemberCommand(Guid Id, string FullName, string Email, string? Phone) : IRequest<ErrorOr<bool>>;

public sealed class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(40);
    }
}

public sealed class UpdateMemberCommandHandler(
    ICurrentTenant tenant,
    IMemberRepository repo,
    IUnitOfWork uow) : IRequestHandler<UpdateMemberCommand, ErrorOr<bool>>
{
    public async Task<ErrorOr<bool>> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        if (tenant.TenantId is not { } tid)
        {
            return Error.Validation("Tenant.Missing", "Tenant is not resolved for this request.");
        }

        var m = await repo.GetAsync(tid, request.Id, cancellationToken);
        if (m is null)
        {
            return Error.NotFound("Member.NotFound", "Member was not found.");
        }

        if (await repo.EmailExistsInTenantAsync(tid, request.Email.Trim(), request.Id, cancellationToken))
        {
            return Error.Conflict("Member.EmailTaken", "A member with this email already exists in this gym.");
        }

        m.FullName = request.FullName.Trim();
        m.Email = request.Email.Trim();
        m.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed record DeleteMemberCommand(Guid Id) : IRequest<ErrorOr<bool>>;

public sealed class DeleteMemberCommandHandler(
    ICurrentTenant tenant,
    IMemberRepository repo,
    IUnitOfWork uow) : IRequestHandler<DeleteMemberCommand, ErrorOr<bool>>
{
    public async Task<ErrorOr<bool>> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        if (tenant.TenantId is not { } tid)
        {
            return Error.Validation("Tenant.Missing", "Tenant is not resolved for this request.");
        }

        var m = await repo.GetAsync(tid, request.Id, cancellationToken);
        if (m is null)
        {
            return Error.NotFound("Member.NotFound", "Member was not found.");
        }

        repo.Remove(m);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}
