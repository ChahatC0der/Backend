using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StaffDocumentEntity = SchoolERP.Domain.Staff.Entities.StaffDocument;

namespace SchoolERP.Application.Features.Staff.Commands.StaffDocument.RestoreStaffDocument;

public class RestoreStaffDocumentCommandHandler : IRequestHandler<RestoreStaffDocumentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public RestoreStaffDocumentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(RestoreStaffDocumentCommand command, CancellationToken cancellationToken)
    {
        var document = await _dbContext.Set<StaffDocumentEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == command.Id && d.IsDeleted, cancellationToken);
        if (document == null)
            return Error.NotFound("StaffDocument", command.Id.ToString());

        document.IsDeleted = false;
        document.DeletedAt = null;
        document.UpdatedAt = DateTime.UtcNow;

        return Result.Success(true);
    }
}
