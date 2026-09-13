using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StudentDocumentEntity = SchoolERP.Domain.Student.Entities.StudentDocument;

namespace SchoolERP.Application.Features.Student.Commands.StudentDocument.DeleteStudentDocument;

public class DeleteStudentDocumentCommandHandler : IRequestHandler<DeleteStudentDocumentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteStudentDocumentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(DeleteStudentDocumentCommand command, CancellationToken cancellationToken)
    {
        var entityResult = await _dbContext.GetEntityAsync<StudentDocumentEntity>(
            d => d.Id == command.Id && !d.IsDeleted,
            "StudentDocument", command.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var document = entityResult.Value;
        document.IsDeleted = true;
        document.DeletedAt = DateTime.UtcNow;
        document.UpdatedAt = DateTime.UtcNow;

        return Result.Success(true);
    }
}