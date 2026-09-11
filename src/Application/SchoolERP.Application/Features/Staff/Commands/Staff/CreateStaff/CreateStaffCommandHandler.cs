using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;
using DepartmentEntity = SchoolERP.Domain.Staff.Entities.Department;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;

namespace SchoolERP.Application.Features.Staff.Commands.Staff.CreateStaff;

public class CreateStaffCommandHandler : IRequestHandler<CreateStaffCommand, Result<StaffResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly IStaffIdGenerator _staffIdGenerator;

    public CreateStaffCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentBranchService branchService,
        IStaffIdGenerator staffIdGenerator)
    {
        _dbContext = dbContext;
        _branchService = branchService;
        _staffIdGenerator = staffIdGenerator;
    }

    public async Task<Result<StaffResponse>> Handle(CreateStaffCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        // Department existence check
        if (request.DepartmentId.HasValue)
        {
            var deptExists = await _dbContext.EnsureEntityExistsAsync<DepartmentEntity>(request.DepartmentId.Value, cancellationToken);
            if (deptExists != null) return Error.NotFound("Department", request.DepartmentId.Value.ToString());
        }

        // User existence check
        if (request.UserId.HasValue)
        {
            var userExists = await _dbContext.EnsureEntityExistsAsync<UserEntity>(request.UserId.Value, cancellationToken);
            if (userExists != null) return Error.NotFound("User", request.UserId.Value.ToString());
        }

        // Auto-generate StaffId
        var staffId = await _staffIdGenerator.GenerateAsync(branchId, cancellationToken);

        var staff = request.Adapt<StaffEntity>();
        staff.StaffId = staffId;
        staff.Status = "active";

        _dbContext.Set<StaffEntity>().Add(staff);

        // Load department name for response (optional)
        DepartmentEntity? department = null;
        if (staff.DepartmentId.HasValue)
        {
            department = await _dbContext.Set<DepartmentEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == staff.DepartmentId.Value, cancellationToken);
        }
        staff.Department = department;

        return Result.Success(staff.Adapt<StaffResponse>());
    }
}
