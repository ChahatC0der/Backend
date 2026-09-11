using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using DepartmentEntity = SchoolERP.Domain.Staff.Entities.Department;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;

namespace SchoolERP.Application.Features.Staff.Commands.Department.CreateDepartment;

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Result<DepartmentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public CreateDepartmentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<DepartmentResponse>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        // Code uniqueness within branch
        var conflict = await _dbContext.EnsureUniqueAsync<DepartmentEntity>(
            d => d.BranchId == branchId && d.Code == request.Code && !d.IsDeleted,
            $"Department with code '{request.Code}' already exists in this branch.",
            cancellationToken);
        if (conflict != null) return conflict;

        // Verify HOD exists if provided
        if (request.HodId.HasValue)
        {
            var hodExists = await _dbContext.EnsureEntityExistsAsync<StaffEntity>(request.HodId.Value, cancellationToken);
            if (hodExists != null) return Error.NotFound("Staff", request.HodId.Value.ToString());
        }

        var department = request.Adapt<DepartmentEntity>();
        _dbContext.Set<DepartmentEntity>().Add(department);

        return Result.Success(department.Adapt<DepartmentResponse>());
    }
}
