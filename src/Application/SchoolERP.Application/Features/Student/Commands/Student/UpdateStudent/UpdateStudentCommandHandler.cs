using Mapster;
using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;

namespace SchoolERP.Application.Features.Student.Commands.Student.UpdateStudent;

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, Result<StudentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public UpdateStudentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<StudentResponse>> Handle(UpdateStudentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var entityResult = await _dbContext.GetEntityAsync<StudentEntity>(
            s => s.Id == request.Id && s.BranchId == branchId && !s.IsDeleted,
            "Student", request.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var student = entityResult.Value;

        if (request.UserId.HasValue)
        {
            var userExists = await _dbContext.EnsureEntityExistsAsync<UserEntity>(request.UserId.Value, cancellationToken);
            if (userExists != null) return Error.NotFound("User", request.UserId.Value.ToString());
        }

        student.FirstName = request.FirstName;
        student.LastName = request.LastName;
        student.DateOfBirth = request.DateOfBirth;
        student.AdmissionDate = request.AdmissionDate;
        student.Gender = request.Gender;
        student.BloodGroup = request.BloodGroup;
        student.Nationality = request.Nationality;
        student.Religion = request.Religion;
        student.MotherTongue = request.MotherTongue;
        student.PersonalEmail = request.PersonalEmail;
        student.PersonalMobile = request.PersonalMobile;
        student.AlternateMobile = request.AlternateMobile;
        student.Address = request.Address;
        student.RollNumber = request.RollNumber;
        student.Status = request.Status;
        student.PhotoUrl = request.PhotoUrl;
        student.CustomFields = request.CustomFields;
        student.UserId = request.UserId;
        student.UpdatedAt = DateTime.UtcNow;

        return Result.Success(student.Adapt<StudentResponse>());
    }
}
