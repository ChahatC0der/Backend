using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using ParentEntity = SchoolERP.Domain.Student.Entities.Parent;

namespace SchoolERP.Application.Features.Student.Commands.Parent.RestoreParent;

public class RestoreParentCommandHandler : IRequestHandler<RestoreParentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public RestoreParentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(RestoreParentCommand command, CancellationToken cancellationToken)
    {
        var parent = await _dbContext.Set<ParentEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == command.Id && p.IsDeleted, cancellationToken);
        if (parent == null)
            return Error.NotFound("Parent", command.Id.ToString());

        parent.IsDeleted = false;
        parent.DeletedAt = null;
        parent.UpdatedAt = DateTime.UtcNow;

        return Result.Success(true);
    }
}
