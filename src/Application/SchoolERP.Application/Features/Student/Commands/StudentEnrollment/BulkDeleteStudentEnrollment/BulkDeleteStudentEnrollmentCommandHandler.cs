using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;

namespace SchoolERP.Application.Features.Student.Commands.StudentEnrollment.BulkDeleteStudentEnrollment;

public class BulkDeleteStudentEnrollmentCommandHandler : IRequestHandler<BulkDeleteStudentEnrollmentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public BulkDeleteStudentEnrollmentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(BulkDeleteStudentEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var entities = await _dbContext.Set<StudentEnrollmentEntity>()
            .Where(e => request.Ids.Contains(e.Id) && !e.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("StudentEnrollment", string.Join(",", request.Ids));

        foreach (var e in entities)
        {
            e.IsDeleted = true;
            e.DeletedAt = DateTime.UtcNow;
            e.UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success(true);
    }
}
