using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using DepartmentEntity = SchoolERP.Domain.Staff.Entities.Department;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;

namespace SchoolERP.Application.Features.Staff.Commands.Department.PatchDepartment;

public class PatchDepartmentCommandHandler : IRequestHandler<PatchDepartmentCommand, Result<DepartmentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public PatchDepartmentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<DepartmentResponse>> Handle(PatchDepartmentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var entityResult = await _dbContext.GetEntityAsync<DepartmentEntity>(
            d => d.Id == request.Id && d.BranchId == branchId && !d.IsDeleted,
            "Department",
            request.Id.ToString(),
            cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var department = entityResult.Value;

        request.Name.PatchIfProvided(value => department.Name = value);
        request.Code.PatchIfProvided(value => department.Code = value);

        if (request.HodId.HasValue)
        {
            var hodExists = await _dbContext.EnsureEntityExistsAsync<StaffEntity>(request.HodId.Value, cancellationToken);
            if (hodExists != null) return Error.NotFound("Staff", request.HodId.Value.ToString());
            department.HodId = request.HodId;
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var conflict = await _dbContext.EnsureUniqueAsync<DepartmentEntity>(
                d => d.BranchId == branchId && d.Code == department.Code && d.Id != department.Id && !d.IsDeleted,
                $"Department with code '{department.Code}' already exists.",
                cancellationToken);
            if (conflict != null) return conflict;
        }

        department.UpdatedAt = DateTime.UtcNow;

        return Result.Success(department.Adapt<DepartmentResponse>());
    }
}
