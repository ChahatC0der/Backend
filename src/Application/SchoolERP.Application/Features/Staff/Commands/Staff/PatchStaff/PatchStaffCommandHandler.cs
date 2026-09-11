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

namespace SchoolERP.Application.Features.Staff.Commands.Staff.PatchStaff;

public class PatchStaffCommandHandler : IRequestHandler<PatchStaffCommand, Result<StaffResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public PatchStaffCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<StaffResponse>> Handle(PatchStaffCommand command, CancellationToken cancellationToken)
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
            staff.DepartmentId = request.DepartmentId;
        }
        if (request.UserId.HasValue)
        {
            var userExists = await _dbContext.EnsureEntityExistsAsync<UserEntity>(request.UserId.Value, cancellationToken);
            if (userExists != null) return Error.NotFound("User", request.UserId.Value.ToString());
            staff.UserId = request.UserId;
        }

        request.FirstName.PatchIfProvided(v => staff.FirstName = v);
        request.LastName.PatchIfProvided(v => staff.LastName = v);
        request.Gender.PatchIfProvided(v => staff.Gender = v);
        request.BloodGroup.PatchIfProvided(v => staff.BloodGroup = v);
        request.MaritalStatus.PatchIfProvided(v => staff.MaritalStatus = v);
        request.Nationality.PatchIfProvided(v => staff.Nationality = v);

        request.PersonalEmail.PatchIfProvided(v => staff.PersonalEmail = v);
        request.WorkEmail.PatchIfProvided(v => staff.WorkEmail = v);
        request.PersonalMobile.PatchIfProvided(v => staff.PersonalMobile = v);
        request.WorkMobile.PatchIfProvided(v => staff.WorkMobile = v);
        request.EmergencyContactName.PatchIfProvided(v => staff.EmergencyContactName = v);
        request.EmergencyContactPhone.PatchIfProvided(v => staff.EmergencyContactPhone = v);

        request.PresentAddress.PatchIfProvided(v => staff.PresentAddress = v);
        request.PermanentAddress.PatchIfProvided(v => staff.PermanentAddress = v);

        request.EmploymentType.PatchIfProvided(v => staff.EmploymentType = v);
        request.StaffType.PatchIfProvided(v => staff.StaffType = v);
        request.Designation.PatchIfProvided(v => staff.Designation = v);

        request.Qualification.PatchIfProvided(v => staff.Qualification = v);
        request.Specialization.PatchIfProvided(v => staff.Specialization = v);
        request.PreviousInstitution.PatchIfProvided(v => staff.PreviousInstitution = v);

        request.PhotoUrl.PatchIfProvided(v => staff.PhotoUrl = v);
        request.CustomFields.PatchIfProvided(v => staff.CustomFields = v);

        if (request.JoiningDate.HasValue) staff.JoiningDate = request.JoiningDate.Value;
        if (request.DateOfBirth.HasValue) staff.DateOfBirth = request.DateOfBirth.Value;
        if (request.ExperienceYears.HasValue) staff.ExperienceYears = request.ExperienceYears.Value;
        if (!string.IsNullOrWhiteSpace(request.Status)) staff.Status = request.Status;

        staff.UpdatedAt = DateTime.UtcNow;

        return Result.Success(staff.Adapt<StaffResponse>());
    }
}
