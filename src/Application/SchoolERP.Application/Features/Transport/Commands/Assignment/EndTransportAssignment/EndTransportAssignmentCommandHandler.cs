using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.Assignment;
using SchoolERP.Domain.Shared.Results;
using TransportAssignmentEntity = SchoolERP.Domain.Transport.Entities.TransportAssignment;

namespace SchoolERP.Application.Features.Transport.Commands.Assignment.EndTransportAssignment;

public sealed class EndTransportAssignmentCommandHandler : IRequestHandler<EndTransportAssignmentCommand, Result<TransportAssignmentResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public EndTransportAssignmentCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    { _db = db; _tenant = tenant; }

    public async Task<Result<TransportAssignmentResponse>> Handle(EndTransportAssignmentCommand request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");

        var entity = await _db.Set<TransportAssignmentEntity>().FirstOrDefaultAsync(a =>
            a.Id == request.AssignmentId && a.TenantId == tenantId && a.IsCurrent && !a.IsDeleted, ct);
        if (entity is null) return Error.NotFound("Current TransportAssignment", request.AssignmentId.ToString());

        var endDate = request.Request.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        if (endDate < entity.ValidFrom)
            return Error.Validation($"EndDate ({endDate:yyyy-MM-dd}) cannot be before ValidFrom ({entity.ValidFrom:yyyy-MM-dd}).");

        entity.IsCurrent = false;
        entity.ValidTo = endDate;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        // TODO (audit): await _auditWriter.LogAsync("TransportAssignments", entity.Id, FinancialAuditAction.Update, null, new { Ended = true, EndDate = endDate }, ct);

        return Result.Success(TransportAssignmentResponse.FromEntity(entity));
    }
}
