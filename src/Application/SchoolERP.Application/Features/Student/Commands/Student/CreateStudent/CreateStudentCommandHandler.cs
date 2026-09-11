using Mapster;
using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;

namespace SchoolERP.Application.Features.Student.Commands.Student.CreateStudent;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Result<StudentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly IEnrollmentIdGenerator _enrollmentIdGenerator;

    public CreateStudentCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentBranchService branchService,
        ICurrentTenantService tenantService,
        IEnrollmentIdGenerator enrollmentIdGenerator)
    {
        _dbContext = dbContext;
        _branchService = branchService;
        _tenantService = tenantService;
        _enrollmentIdGenerator = enrollmentIdGenerator;
    }

    public async Task<Result<StudentResponse>> Handle(CreateStudentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;
        var tenantId = _tenantService.GetTenantId();

        if (request.UserId.HasValue)
        {
            var userExists = await _dbContext.EnsureEntityExistsAsync<UserEntity>(request.UserId.Value, cancellationToken);
            if (userExists != null) return Error.NotFound("User", request.UserId.Value.ToString());
        }

        var enrollmentId = await _enrollmentIdGenerator.GenerateAsync(branchId, cancellationToken);

        var student = request.Adapt<StudentEntity>();
        student.TenantId = tenantId;
        student.EnrollmentId = enrollmentId;

        _dbContext.Set<StudentEntity>().Add(student);

        return Result.Success(student.Adapt<StudentResponse>());
    }
}
