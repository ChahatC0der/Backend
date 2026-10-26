using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Shared.Results;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;

namespace SchoolERP.Application.Features.Fees.Queries.Invoice.GetInvoices;

public sealed class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, Result<PagedResponse<InvoiceListResponse>>>
{
    private static readonly HashSet<string> AllowedSort = new(StringComparer.OrdinalIgnoreCase)
        { "DueDate", "CreatedAt", "NetPayable", "TotalAmount", "Status" };

    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetInvoicesQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<PagedResponse<InvoiceListResponse>>> Handle(GetInvoicesQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var page = request.Page <= 0 ? 1 : request.Page;
        var size = request.Size <= 0 ? 10 : Math.Min(request.Size, 200);

        var query = _db.Set<InvoiceEntity>().AsNoTracking()
            .Where(i => i.TenantId == tenantId && i.BranchId == branchId && i.DeletedAt == null);

        if (request.AcademicYearId.HasValue) query = query.Where(i => i.AcademicYearId == request.AcademicYearId.Value);
        if (request.StudentId.HasValue) query = query.Where(i => i.StudentId == request.StudentId.Value);
        if (request.ClassId.HasValue) query = query.Where(i => i.ClassId == request.ClassId.Value);
        if (request.SectionId.HasValue) query = query.Where(i => i.SectionId == request.SectionId.Value);
        if (request.Status.HasValue) query = query.Where(i => i.Status == request.Status.Value);
        if (!string.IsNullOrWhiteSpace(request.PeriodLabel))
            query = query.Where(i => i.PeriodLabel.Contains(request.PeriodLabel.Trim()));
        if (request.DueDateFrom.HasValue) query = query.Where(i => i.DueDate >= request.DueDateFrom.Value);
        if (request.DueDateTo.HasValue) query = query.Where(i => i.DueDate <= request.DueDateTo.Value);

        var sortBy = AllowedSort.Contains(request.SortBy ?? "") ? request.SortBy! : "DueDate";
        var desc = string.Equals(request.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = sortBy switch
        {
            "CreatedAt"  => desc ? query.OrderByDescending(i => i.CreatedAt)  : query.OrderBy(i => i.CreatedAt),
            "NetPayable" => desc ? query.OrderByDescending(i => i.NetPayable) : query.OrderBy(i => i.NetPayable),
            "TotalAmount" => desc ? query.OrderByDescending(i => i.TotalAmount) : query.OrderBy(i => i.TotalAmount),
            "Status"     => desc ? query.OrderByDescending(i => i.Status)     : query.OrderBy(i => i.Status),
            _            => desc ? query.OrderByDescending(i => i.DueDate)    : query.OrderBy(i => i.DueDate),
        };

        var total = await query.CountAsync(ct);
        var rows = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        return Result.Success(new PagedResponse<InvoiceListResponse>
        {
            Data = rows.Select(InvoiceResponseFactory.ToListResponse).ToList(),
            Page = page, Size = size, TotalCount = total,
            //TotalPages = (int)Math.Ceiling(total / (double)size),
        });
    }
}
