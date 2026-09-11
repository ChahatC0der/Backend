using Mapster;
using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;

namespace SchoolERP.Application.Features.Student.Commands.Student.PatchStudent;

public class PatchStudentCommandHandler : IRequestHandler<PatchStudentCommand, Result<StudentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public PatchStudentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<StudentResponse>> Handle(PatchStudentCommand command, CancellationToken cancellationToken)
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
            student.UserId = request.UserId;
        }

        request.FirstName.PatchIfProvided(v => student.FirstName = v);
        request.LastName.PatchIfProvided(v => student.LastName = v);
        request.Gender.PatchIfProvided(v => student.Gender = v);
        request.BloodGroup.PatchIfProvided(v => student.BloodGroup = v);
        request.Nationality.PatchIfProvided(v => student.Nationality = v);
        request.Religion.PatchIfProvided(v => student.Religion = v);
        request.MotherTongue.PatchIfProvided(v => student.MotherTongue = v);
        request.PersonalEmail.PatchIfProvided(v => student.PersonalEmail = v);
        request.PersonalMobile.PatchIfProvided(v => student.PersonalMobile = v);
        request.AlternateMobile.PatchIfProvided(v => student.AlternateMobile = v);
        request.Address.PatchIfProvided(v => student.Address = v);
        request.RollNumber.PatchIfProvided(v => student.RollNumber = v);
        request.PhotoUrl.PatchIfProvided(v => student.PhotoUrl = v);
        request.CustomFields.PatchIfProvided(v => student.CustomFields = v);
        request.Status.PatchIfProvided(v => student.Status = v);

        if (request.DateOfBirth.HasValue) student.DateOfBirth = request.DateOfBirth.Value;
        if (request.AdmissionDate.HasValue) student.AdmissionDate = request.AdmissionDate.Value;

        student.UpdatedAt = DateTime.UtcNow;

        return Result.Success(student.Adapt<StudentResponse>());
    }
}
