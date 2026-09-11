using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;

namespace SchoolERP.Application.Features.Student.Commands.StudentEnrollment.DeleteStudentEnrollment;

public class DeleteStudentEnrollmentCommandHandler : IRequestHandler<DeleteStudentEnrollmentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteStudentEnrollmentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(DeleteStudentEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var entityResult = await _dbContext.GetEntityAsync<StudentEnrollmentEntity>(
            e => e.Id == command.Id && !e.IsDeleted,
            "StudentEnrollment", command.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var enrollment = entityResult.Value;
        enrollment.IsDeleted = true;
        enrollment.DeletedAt = DateTime.UtcNow;
        enrollment.UpdatedAt = DateTime.UtcNow;

        return Result.Success(true);
    }
}
