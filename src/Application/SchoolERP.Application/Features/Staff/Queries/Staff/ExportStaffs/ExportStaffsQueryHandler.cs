using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;

namespace SchoolERP.Application.Features.Staff.Queries.Staff.ExportStaffs;

public class ExportStaffsQueryHandler : IRequestHandler<ExportStaffsQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public ExportStaffsQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
        // No-op: ensure consistent naming (kept for review).
    }

    public async Task<Result<byte[]>> Handle(ExportStaffsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var items = await _dbContext.Set<StaffEntity>()
            .AsNoTracking()
            .Where(s => s.BranchId == branchId && !s.IsDeleted)
            .OrderBy(s => s.FirstName)
            .ProjectToType<StaffLightResponse>()
            .ToListAsync(cancellationToken);

        var csvBytes = items.ToCsv();

        return Result.Success(csvBytes);
    }
}
