using ErrorOr;
using FluentValidation;
using GymSaaS.Application.Abstractions;
using GymSaaS.Application.Abstractions.Persistence;
using GymSaaS.Domain.Entities;
using MediatR;

namespace GymSaaS.Application.Features.Facilities;

public sealed record FacilityDto(Guid Id, string Name, string? AddressLine, DateTimeOffset CreatedAtUtc);

public sealed record ListFacilitiesQuery : IRequest<ErrorOr<IReadOnlyList<FacilityDto>>>;

public sealed class ListFacilitiesQueryHandler(ICurrentTenant tenant, IFacilityRepository repo)
    : IRequestHandler<ListFacilitiesQuery, ErrorOr<IReadOnlyList<FacilityDto>>>
{
    public async Task<ErrorOr<IReadOnlyList<FacilityDto>>> Handle(
        ListFacilitiesQuery request,
        CancellationToken cancellationToken)
    {
        if (tenant.TenantId is not { } tid)
        {
            return Error.Validation("Tenant.Missing", "Tenant is not resolved for this request.");
        }

        var list = await repo.ListAsync(tid, cancellationToken);
        return list.Select(f => new FacilityDto(f.Id, f.Name, f.AddressLine, f.CreatedAtUtc)).ToList();
    }
}

public sealed record GetFacilityQuery(Guid Id) : IRequest<ErrorOr<FacilityDto>>;

public sealed class GetFacilityQueryHandler(ICurrentTenant tenant, IFacilityRepository repo)
    : IRequestHandler<GetFacilityQuery, ErrorOr<FacilityDto>>
{
    public async Task<ErrorOr<FacilityDto>> Handle(GetFacilityQuery request, CancellationToken cancellationToken)
    {
        if (tenant.TenantId is not { } tid)
        {
            return Error.Validation("Tenant.Missing", "Tenant is not resolved for this request.");
        }

        var f = await repo.GetAsync(tid, request.Id, cancellationToken);
        if (f is null)
        {
            return Error.NotFound("Facility.NotFound", "Facility was not found.");
        }

        return new FacilityDto(f.Id, f.Name, f.AddressLine, f.CreatedAtUtc);
    }
}

public sealed record CreateFacilityCommand(string Name, string? AddressLine) : IRequest<ErrorOr<Guid>>;

public sealed class CreateFacilityCommandValidator : AbstractValidator<CreateFacilityCommand>
{
    public CreateFacilityCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AddressLine).MaximumLength(400);
    }
}

public sealed class CreateFacilityCommandHandler(
    ICurrentTenant tenant,
    IFacilityRepository repo,
    IUnitOfWork uow) : IRequestHandler<CreateFacilityCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateFacilityCommand request, CancellationToken cancellationToken)
    {
        if (tenant.TenantId is not { } tid)
        {
            return Error.Validation("Tenant.Missing", "Tenant is not resolved for this request.");
        }

        var entity = new Facility
        {
            Id = Guid.NewGuid(),
            TenantId = tid,
            Name = request.Name.Trim(),
            AddressLine = string.IsNullOrWhiteSpace(request.AddressLine) ? null : request.AddressLine.Trim(),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        await repo.AddAsync(entity, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}

public sealed record UpdateFacilityCommand(Guid Id, string Name, string? AddressLine) : IRequest<ErrorOr<bool>>;

public sealed class UpdateFacilityCommandValidator : AbstractValidator<UpdateFacilityCommand>
{
    public UpdateFacilityCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AddressLine).MaximumLength(400);
    }
}

public sealed class UpdateFacilityCommandHandler(
    ICurrentTenant tenant,
    IFacilityRepository repo,
    IUnitOfWork uow) : IRequestHandler<UpdateFacilityCommand, ErrorOr<bool>>
{
    public async Task<ErrorOr<bool>> Handle(UpdateFacilityCommand request, CancellationToken cancellationToken)
    {
        if (tenant.TenantId is not { } tid)
        {
            return Error.Validation("Tenant.Missing", "Tenant is not resolved for this request.");
        }

        var f = await repo.GetAsync(tid, request.Id, cancellationToken);
        if (f is null)
        {
            return Error.NotFound("Facility.NotFound", "Facility was not found.");
        }

        f.Name = request.Name.Trim();
        f.AddressLine = string.IsNullOrWhiteSpace(request.AddressLine) ? null : request.AddressLine.Trim();
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed record DeleteFacilityCommand(Guid Id) : IRequest<ErrorOr<bool>>;

public sealed class DeleteFacilityCommandHandler(
    ICurrentTenant tenant,
    IFacilityRepository repo,
    IUnitOfWork uow) : IRequestHandler<DeleteFacilityCommand, ErrorOr<bool>>
{
    public async Task<ErrorOr<bool>> Handle(DeleteFacilityCommand request, CancellationToken cancellationToken)
    {
        if (tenant.TenantId is not { } tid)
        {
            return Error.Validation("Tenant.Missing", "Tenant is not resolved for this request.");
        }

        var f = await repo.GetAsync(tid, request.Id, cancellationToken);
        if (f is null)
        {
            return Error.NotFound("Facility.NotFound", "Facility was not found.");
        }

        repo.Remove(f);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}
