using ErrorOr;
using FluentValidation;
using GymSaaS.Application.Abstractions;
using GymSaaS.Application.Abstractions.Persistence;
using GymSaaS.Domain.Entities;
using MediatR;

namespace GymSaaS.Application.Features.Rooms;

public sealed record RoomDto(
    Guid Id,
    Guid FacilityId,
    string FacilityName,
    string Name,
    int Capacity,
    DateTimeOffset CreatedAtUtc);

public sealed record ListRoomsQuery : IRequest<ErrorOr<IReadOnlyList<RoomDto>>>;

public sealed class ListRoomsQueryHandler(ICurrentTenant tenant, IRoomRepository repo)
    : IRequestHandler<ListRoomsQuery, ErrorOr<IReadOnlyList<RoomDto>>>
{
    public async Task<ErrorOr<IReadOnlyList<RoomDto>>> Handle(
        ListRoomsQuery request,
        CancellationToken cancellationToken)
    {
        if (tenant.TenantId is not { } tid)
        {
            return Error.Validation("Tenant.Missing", "Tenant is not resolved for this request.");
        }

        var list = await repo.ListAsync(tid, cancellationToken);
        return list.Select(r => new RoomDto(
                r.Id,
                r.FacilityId,
                r.Facility.Name,
                r.Name,
                r.Capacity,
                r.CreatedAtUtc))
            .ToList();
    }
}

public sealed record GetRoomQuery(Guid Id) : IRequest<ErrorOr<RoomDto>>;

public sealed class GetRoomQueryHandler(ICurrentTenant tenant, IRoomRepository repo)
    : IRequestHandler<GetRoomQuery, ErrorOr<RoomDto>>
{
    public async Task<ErrorOr<RoomDto>> Handle(GetRoomQuery request, CancellationToken cancellationToken)
    {
        if (tenant.TenantId is not { } tid)
        {
            return Error.Validation("Tenant.Missing", "Tenant is not resolved for this request.");
        }

        var r = await repo.GetAsync(tid, request.Id, cancellationToken);
        if (r is null)
        {
            return Error.NotFound("Room.NotFound", "Room was not found.");
        }

        return new RoomDto(r.Id, r.FacilityId, r.Facility.Name, r.Name, r.Capacity, r.CreatedAtUtc);
    }
}

public sealed record CreateRoomCommand(Guid FacilityId, string Name, int Capacity) : IRequest<ErrorOr<Guid>>;

public sealed class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(x => x.FacilityId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Capacity).GreaterThan(0).LessThanOrEqualTo(10_000);
    }
}

public sealed class CreateRoomCommandHandler(
    ICurrentTenant tenant,
    IRoomRepository repo,
    IUnitOfWork uow) : IRequestHandler<CreateRoomCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        if (tenant.TenantId is not { } tid)
        {
            return Error.Validation("Tenant.Missing", "Tenant is not resolved for this request.");
        }

        if (!await repo.FacilityBelongsToTenantAsync(tid, request.FacilityId, cancellationToken))
        {
            return Error.NotFound("Facility.NotFound", "Facility was not found for this tenant.");
        }

        var entity = new Room
        {
            Id = Guid.NewGuid(),
            TenantId = tid,
            FacilityId = request.FacilityId,
            Name = request.Name.Trim(),
            Capacity = request.Capacity,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        await repo.AddAsync(entity, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}

public sealed record UpdateRoomCommand(Guid Id, string Name, int Capacity) : IRequest<ErrorOr<bool>>;

public sealed class UpdateRoomCommandValidator : AbstractValidator<UpdateRoomCommand>
{
    public UpdateRoomCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Capacity).GreaterThan(0).LessThanOrEqualTo(10_000);
    }
}

public sealed class UpdateRoomCommandHandler(
    ICurrentTenant tenant,
    IRoomRepository repo,
    IUnitOfWork uow) : IRequestHandler<UpdateRoomCommand, ErrorOr<bool>>
{
    public async Task<ErrorOr<bool>> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
    {
        if (tenant.TenantId is not { } tid)
        {
            return Error.Validation("Tenant.Missing", "Tenant is not resolved for this request.");
        }

        var r = await repo.GetAsync(tid, request.Id, cancellationToken);
        if (r is null)
        {
            return Error.NotFound("Room.NotFound", "Room was not found.");
        }

        r.Name = request.Name.Trim();
        r.Capacity = request.Capacity;
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed record DeleteRoomCommand(Guid Id) : IRequest<ErrorOr<bool>>;

public sealed class DeleteRoomCommandHandler(
    ICurrentTenant tenant,
    IRoomRepository repo,
    IUnitOfWork uow) : IRequestHandler<DeleteRoomCommand, ErrorOr<bool>>
{
    public async Task<ErrorOr<bool>> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        if (tenant.TenantId is not { } tid)
        {
            return Error.Validation("Tenant.Missing", "Tenant is not resolved for this request.");
        }

        var r = await repo.GetAsync(tid, request.Id, cancellationToken);
        if (r is null)
        {
            return Error.NotFound("Room.NotFound", "Room was not found.");
        }

        repo.Remove(r);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}
