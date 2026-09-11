using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StaffDocumentEntity = SchoolERP.Domain.Staff.Entities.StaffDocument;

namespace SchoolERP.Application.Features.Staff.Commands.StaffDocument.BulkUpdateStaffDocument;

public class BulkUpdateStaffDocumentCommandHandler : IRequestHandler<BulkUpdateStaffDocumentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public BulkUpdateStaffDocumentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(BulkUpdateStaffDocumentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var entities = await _dbContext.Set<StaffDocumentEntity>()
            .Where(d => request.Ids.Contains(d.Id) && !d.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("StaffDocument", string.Join(",", request.Ids));

        foreach (var d in entities)
        {
            d.DocumentType = request.DocumentType;
            d.VerificationStatus = request.VerificationStatus;
            d.VerifiedAt = request.VerificationStatus == "verified" ? DateTime.UtcNow : null;
            d.UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success(true);
    }
}
