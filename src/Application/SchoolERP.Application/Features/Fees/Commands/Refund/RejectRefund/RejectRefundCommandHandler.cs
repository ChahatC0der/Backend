using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Refund;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using RefundEntity = SchoolERP.Domain.Fees.Entities.Refund;

namespace SchoolERP.Application.Features.Fees.Commands.Refund.RejectRefund;

public sealed class RejectRefundCommandHandler : IRequestHandler<RejectRefundCommand, Result<RefundResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public RejectRefundCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    { _db = db; _tenant = tenant; }

    public async Task<Result<RefundResponse>> Handle(RejectRefundCommand request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");

        var entity = await _db.Set<RefundEntity>().FirstOrDefaultAsync(r =>
            r.Id == request.RefundId && r.TenantId == tenantId, ct);
        if (entity is null) return Error.NotFound("Refund", request.RefundId.ToString());

        if (entity.Status != RefundStatus.Pending)
            return Error.Conflict($"Cannot reject a refund in status {entity.Status}.");

        entity.Status = RefundStatus.Rejected;
        entity.RejectionReason = request.Request.RejectionReason.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Refunds", entity.Id, FinancialAuditAction.Update, null, new { Status = "rejected" }, ct);

        return Result.Success(RefundResponse.FromEntity(entity));
    }
}
