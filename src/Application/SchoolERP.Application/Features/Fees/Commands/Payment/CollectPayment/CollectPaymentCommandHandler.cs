using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Fees.DTOs.Payment;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Payment.CollectPayment;

public sealed class CollectPaymentCommandHandler : IRequestHandler<CollectPaymentCommand, Result<PaymentResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentUserService _user;
    private readonly ICurrentAcademicYearProvider _ay;
    private readonly IFeeSequenceGenerator _sequence;
    private readonly IWalletLedgerWriter _wallet;

    public CollectPaymentCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch,
        ICurrentUserService user, ICurrentAcademicYearProvider ay,
        IFeeSequenceGenerator sequence, IWalletLedgerWriter wallet)
    {
        _db = db; _tenant = tenant; _branch = branch; _user = user;
        _ay = ay; _sequence = sequence; _wallet = wallet;
    }

    public async Task<Result<PaymentResponse>> Handle(CollectPaymentCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var ay = await _ay.GetByIdAsync(request.Request.AcademicYearId, ct);
        if (ay is null) return Error.NotFound("AcademicYear", request.Request.AcademicYearId.ToString());

        var buildResult = await PaymentCollector.BuildAsync(_db, _sequence, _user.GetUserId(),
            tenantId, branchId, ay.Code, request.Request, ct);
        if (buildResult.IsFailure) return buildResult.Error!;

        await _db.SaveChangesAsync(ct);   // populate Payment.Id

        var finalResult = await PaymentCollector.FinalizeAsync(_db, _wallet, buildResult.Value, ct);
        if (finalResult.IsFailure) return finalResult.Error!;

        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Payments", buildResult.Value.Payment.Id, FinancialAuditAction.Create, null, buildResult.Value.Payment, ct);

        return Result.Success(PaymentResponse.FromEntity(buildResult.Value.Payment));
    }
}
