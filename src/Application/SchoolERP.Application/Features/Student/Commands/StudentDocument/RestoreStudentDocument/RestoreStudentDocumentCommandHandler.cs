using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StudentDocumentEntity = SchoolERP.Domain.Student.Entities.StudentDocument;

namespace SchoolERP.Application.Features.Student.Commands.StudentDocument.RestoreStudentDocument;

public class RestoreStudentDocumentCommandHandler : IRequestHandler<RestoreStudentDocumentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public RestoreStudentDocumentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(RestoreStudentDocumentCommand command, CancellationToken cancellationToken)
    {
        var document = await _dbContext.Set<StudentDocumentEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == command.Id && d.IsDeleted, cancellationToken);
        if (document == null)
            return Error.NotFound("StudentDocument", command.Id.ToString());

        document.IsDeleted = false;
        document.DeletedAt = null;
        document.UpdatedAt = DateTime.UtcNow;

        return Result.Success(true);
    }
}