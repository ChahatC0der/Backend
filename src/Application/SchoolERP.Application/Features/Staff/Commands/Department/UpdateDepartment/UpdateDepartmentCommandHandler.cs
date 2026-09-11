using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using DepartmentEntity = SchoolERP.Domain.Staff.Entities.Department;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;

namespace SchoolERP.Application.Features.Staff.Commands.Department.UpdateDepartment;

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, Result<DepartmentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public UpdateDepartmentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<DepartmentResponse>> Handle(UpdateDepartmentCommand command, CancellationToken cancellationToken)
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

        var conflict = await _dbContext.EnsureUniqueAsync<DepartmentEntity>(
            d => d.BranchId == branchId && d.Code == request.Code && d.Id != request.Id && !d.IsDeleted,
            $"Department with code '{request.Code}' already exists in this branch.",
            cancellationToken);
        if (conflict != null) return conflict;

        if (request.HodId.HasValue)
        {
            var hodExists = await _dbContext.EnsureEntityExistsAsync<StaffEntity>(request.HodId.Value, cancellationToken);
            if (hodExists != null) return Error.NotFound("Staff", request.HodId.Value.ToString());
        }

        department.Name = request.Name;
        department.Code = request.Code;
        department.HodId = request.HodId;
        department.UpdatedAt = DateTime.UtcNow;

        return Result.Success(department.Adapt<DepartmentResponse>());
    }
}
