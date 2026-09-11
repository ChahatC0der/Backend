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

namespace SchoolERP.Application.Features.Staff.Commands.Staff.UpdateStaff;

public class UpdateStaffCommandHandler : IRequestHandler<UpdateStaffCommand, Result<StaffResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public UpdateStaffCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<StaffResponse>> Handle(UpdateStaffCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var entityResult = await _dbContext.GetEntityAsync<StaffEntity>(
            s => s.Id == request.Id && s.BranchId == branchId && !s.IsDeleted,
            "Staff", request.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var staff = entityResult.Value;

        if (request.DepartmentId.HasValue)
        {
            var deptExists = await _dbContext.EnsureEntityExistsAsync<DepartmentEntity>(request.DepartmentId.Value, cancellationToken);
            if (deptExists != null) return Error.NotFound("Department", request.DepartmentId.Value.ToString());
        }
        if (request.UserId.HasValue)
        {
            var userExists = await _dbContext.EnsureEntityExistsAsync<UserEntity>(request.UserId.Value, cancellationToken);
            if (userExists != null) return Error.NotFound("User", request.UserId.Value.ToString());
        }

        staff.FirstName = request.FirstName;
        staff.LastName = request.LastName;
        staff.DateOfBirth = request.DateOfBirth;
        staff.Gender = request.Gender;
        staff.BloodGroup = request.BloodGroup;
        staff.MaritalStatus = request.MaritalStatus;
        staff.Nationality = request.Nationality;

        staff.PersonalEmail = request.PersonalEmail;
        staff.WorkEmail = request.WorkEmail;
        staff.PersonalMobile = request.PersonalMobile;
        staff.WorkMobile = request.WorkMobile;
        staff.EmergencyContactName = request.EmergencyContactName;
        staff.EmergencyContactPhone = request.EmergencyContactPhone;

        staff.PresentAddress = request.PresentAddress;
        staff.PermanentAddress = request.PermanentAddress;

        staff.JoiningDate = request.JoiningDate;
        staff.EmploymentType = request.EmploymentType;
        staff.StaffType = request.StaffType;
        staff.Designation = request.Designation;
        staff.DepartmentId = request.DepartmentId;
        staff.UserId = request.UserId;

        staff.Qualification = request.Qualification;
        staff.Specialization = request.Specialization;
        staff.ExperienceYears = request.ExperienceYears;
        staff.PreviousInstitution = request.PreviousInstitution;

        staff.PhotoUrl = request.PhotoUrl;
        staff.Status = request.Status ?? staff.Status;
        staff.CustomFields = request.CustomFields;
        staff.UpdatedAt = DateTime.UtcNow;

        return Result.Success(staff.Adapt<StaffResponse>());
    }
}
