using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.FeeHead;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.FeeHead.ExportFeeHeads;

public sealed class ExportFeeHeadsQueryHandler : IRequestHandler<ExportFeeHeadsQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly IExcelExportService _excel;

    public ExportFeeHeadsQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant,
                                      ICurrentBranchService branch, IExcelExportService excel)
    {
        _db = db; _tenant = tenant; _branch = branch; _excel = excel;
    }

    public async Task<Result<byte[]>> Handle(ExportFeeHeadsQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var query = _db.Set<FeeHeadEntity>().AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.BranchId == branchId && x.DeletedAt == null);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);

        var rows = await query.OrderBy(x => x.Name)
            .Select(x => new FeeHeadExportRow
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                Status = x.Status.ToString(),
                IsMandatory = x.IsMandatory ? "Yes" : "No",
                IsOptional = x.IsOptional ? "Yes" : "No",
                IsRefundable = x.IsRefundable ? "Yes" : "No",
                CreatedAt = x.CreatedAt,
            })
            .ToListAsync(ct);

        var bytes = _excel.Export(rows, "Fee Heads");
        return Result.Success(bytes);
    }
}
