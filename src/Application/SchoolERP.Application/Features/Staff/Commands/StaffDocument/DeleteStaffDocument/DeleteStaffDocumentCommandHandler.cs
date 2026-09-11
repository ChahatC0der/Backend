using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StaffDocumentEntity = SchoolERP.Domain.Staff.Entities.StaffDocument;

namespace SchoolERP.Application.Features.Staff.Commands.StaffDocument.DeleteStaffDocument;

public class DeleteStaffDocumentCommandHandler : IRequestHandler<DeleteStaffDocumentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteStaffDocumentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(DeleteStaffDocumentCommand command, CancellationToken cancellationToken)
    {
        var entityResult = await _dbContext.GetEntityAsync<StaffDocumentEntity>(
            d => d.Id == command.Id && !d.IsDeleted,
            "StaffDocument", command.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var document = entityResult.Value;
        document.IsDeleted = true;
        document.DeletedAt = DateTime.UtcNow;
        document.UpdatedAt = DateTime.UtcNow;

        return Result.Success(true);
    }
}
