using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.Route;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Transport.Enums;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;

namespace SchoolERP.Application.Features.Transport.Commands.Route.CreateRoute;

public sealed class CreateRouteCommandHandler : IRequestHandler<CreateRouteCommand, Result<RouteResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public CreateRouteCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<RouteResponse>> Handle(CreateRouteCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var code = request.Request.Code.Trim().ToUpperInvariant();

        var duplicate = await _db.Set<RouteEntity>().AnyAsync(r =>
            r.BranchId == branchId && !r.IsDeleted && r.Code == code, ct);
        if (duplicate) return Error.Conflict($"Route code {code} already exists in this branch.");

        var entity = new RouteEntity
        {
            BranchId = branchId,
            Name = request.Request.Name.Trim(),
            Code = code,
            Description = request.Request.Description?.Trim(),
            Status = RouteStatus.Active,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Set<RouteEntity>().Add(entity);
        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Routes", entity.Id, FinancialAuditAction.Create, null, entity, ct);

        return Result.Success(RouteResponse.FromEntity(entity));
    }
}
