using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using DepartmentEntity = SchoolERP.Domain.Staff.Entities.Department;

namespace SchoolERP.Application.Features.Staff.Queries.Department.ExportDepartments;

public class ExportDepartmentsQueryHandler : IRequestHandler<ExportDepartmentsQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public ExportDepartmentsQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<byte[]>> Handle(ExportDepartmentsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var items = await _dbContext.Set<DepartmentEntity>()
            .AsNoTracking()
            .Where(d => d.BranchId == branchId && !d.IsDeleted)
            .OrderBy(d => d.Name)
            .ProjectToType<DepartmentLightResponse>()
            .ToListAsync(cancellationToken);

        var csvBytes = items.ToCsv();

        return Result.Success(csvBytes);
    }
}
