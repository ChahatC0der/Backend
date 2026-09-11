using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StaffDocumentEntity = SchoolERP.Domain.Staff.Entities.StaffDocument;

namespace SchoolERP.Application.Features.Staff.Commands.StaffDocument.BulkDeleteStaffDocument;

public class BulkDeleteStaffDocumentCommandHandler : IRequestHandler<BulkDeleteStaffDocumentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public BulkDeleteStaffDocumentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(BulkDeleteStaffDocumentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var entities = await _dbContext.Set<StaffDocumentEntity>()
            .Where(d => request.Ids.Contains(d.Id) && !d.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("StaffDocument", string.Join(",", request.Ids));

        foreach (var d in entities)
        {
            d.IsDeleted = true;
            d.DeletedAt = DateTime.UtcNow;
            d.UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success(true);
    }
}
