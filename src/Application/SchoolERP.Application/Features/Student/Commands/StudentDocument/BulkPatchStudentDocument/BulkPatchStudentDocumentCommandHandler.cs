using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StudentDocumentEntity = SchoolERP.Domain.Student.Entities.StudentDocument;

namespace SchoolERP.Application.Features.Student.Commands.StudentDocument.BulkPatchStudentDocument;

public class BulkPatchStudentDocumentCommandHandler : IRequestHandler<BulkPatchStudentDocumentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public BulkPatchStudentDocumentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(BulkPatchStudentDocumentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var entities = await _dbContext.Set<StudentDocumentEntity>()
            .Where(d => request.Ids.Contains(d.Id) && !d.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("StudentDocument", string.Join(",", request.Ids));

        foreach (var d in entities)
        {
            if (request.Verified.HasValue) d.Verified = request.Verified.Value;
            d.UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success(true);
    }
}