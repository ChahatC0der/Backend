using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Shared.Results;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;

namespace SchoolERP.Application.Features.Fees.Queries.Invoice.GetInvoiceById;

public sealed class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetInvoiceByIdQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<InvoiceResponse>> Handle(GetInvoiceByIdQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var invoice = await _db.Set<InvoiceEntity>().AsNoTracking()
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == request.Id && i.TenantId == tenantId
                                   && i.BranchId == branchId && i.DeletedAt == null, ct);
        if (invoice is null) return Error.NotFound("Invoice", request.Id.ToString());

        return Result.Success(InvoiceResponseFactory.ToResponse(invoice));
    }
}
