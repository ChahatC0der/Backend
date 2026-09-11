using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;

namespace SchoolERP.Application.Features.Student.Commands.StudentEnrollment.RestoreStudentEnrollment;

public class RestoreStudentEnrollmentCommandHandler : IRequestHandler<RestoreStudentEnrollmentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public RestoreStudentEnrollmentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(RestoreStudentEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var enrollment = await _dbContext.Set<StudentEnrollmentEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == command.Id && e.IsDeleted, cancellationToken);
        if (enrollment == null)
            return Error.NotFound("StudentEnrollment", command.Id.ToString());

        enrollment.IsDeleted = false;
        enrollment.DeletedAt = null;
        enrollment.UpdatedAt = DateTime.UtcNow;

        return Result.Success(true);
    }
}
