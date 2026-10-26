using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Refund;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using RefundEntity = SchoolERP.Domain.Fees.Entities.Refund;

namespace SchoolERP.Application.Features.Fees.Commands.Refund.ApproveRefund;

public sealed class ApproveRefundCommandHandler : IRequestHandler<ApproveRefundCommand, Result<RefundResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentUserService _user;

    public ApproveRefundCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentUserService user)
    { _db = db; _tenant = tenant; _user = user; }

    public async Task<Result<RefundResponse>> Handle(ApproveRefundCommand request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");

        var entity = await _db.Set<RefundEntity>().FirstOrDefaultAsync(r =>
            r.Id == request.RefundId && r.TenantId == tenantId, ct);
        if (entity is null) return Error.NotFound("Refund", request.RefundId.ToString());

        if (entity.Status != RefundStatus.Pending)
            return Error.Conflict($"Cannot approve a refund in status {entity.Status}.");

        entity.Status = RefundStatus.Approved;
        entity.ApprovedBy = _user.GetUserId();
        entity.ApprovedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Refunds", entity.Id, FinancialAuditAction.Update, null, new { Status = "approved" }, ct);

        return Result.Success(RefundResponse.FromEntity(entity));
    }
}
