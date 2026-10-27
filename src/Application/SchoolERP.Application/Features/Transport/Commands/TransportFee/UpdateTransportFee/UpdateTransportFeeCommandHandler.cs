using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.Commands.TransportFee.CreateTransportFee;
using SchoolERP.Application.Features.Transport.DTOs.TransportFee;
using SchoolERP.Domain.Shared.Results;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;
using StopEntity = SchoolERP.Domain.Transport.Entities.Stop;
using TransportFeeEntity = SchoolERP.Domain.Transport.Entities.TransportFee;

namespace SchoolERP.Application.Features.Transport.Commands.TransportFee.UpdateTransportFee;

public sealed class UpdateTransportFeeCommandHandler : IRequestHandler<UpdateTransportFeeCommand, Result<TransportFeeResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public UpdateTransportFeeCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<TransportFeeResponse>> Handle(UpdateTransportFeeCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var entity = await _db.Set<TransportFeeEntity>().FirstOrDefaultAsync(f =>
            f.Id == request.Id && f.BranchId == branchId && !f.IsDeleted, ct);
        if (entity is null) return Error.NotFound("TransportFee", request.Id.ToString());

        if (!CreateTransportFeeCommandHandler.TryParseFrequency(request.Request.Frequency, out var freq))
            return Error.Validation($"Invalid Frequency: {request.Request.Frequency}.");

        entity.FeeAmount = request.Request.FeeAmount;
        entity.Frequency = freq;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        // TODO (audit): await _auditWriter.LogAsync("TransportFees", entity.Id, FinancialAuditAction.Update, null, entity, ct);

        // Fetch names for response
        string? routeName = null, routeCode = null, stopName = null;
        if (entity.RouteId.HasValue)
        {
            var r = await _db.Set<RouteEntity>().AsNoTracking().Where(x => x.Id == entity.RouteId.Value)
                .Select(x => new { x.Name, x.Code }).FirstOrDefaultAsync(ct);
            routeName = r?.Name; routeCode = r?.Code;
        }
        else if (entity.StopId.HasValue)
        {
            stopName = await _db.Set<StopEntity>().AsNoTracking().Where(s => s.Id == entity.StopId.Value)
                .Select(s => s.Name).FirstOrDefaultAsync(ct);
        }

        return Result.Success(TransportFeeResponse.FromEntity(entity, routeName, routeCode, stopName));
    }
}
