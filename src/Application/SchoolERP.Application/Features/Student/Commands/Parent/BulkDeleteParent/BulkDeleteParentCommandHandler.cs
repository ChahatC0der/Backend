using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using ParentEntity = SchoolERP.Domain.Student.Entities.Parent;

namespace SchoolERP.Application.Features.Student.Commands.Parent.BulkDeleteParent;

public class BulkDeleteParentCommandHandler : IRequestHandler<BulkDeleteParentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public BulkDeleteParentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(BulkDeleteParentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var entities = await _dbContext.Set<ParentEntity>()
            .Where(p => request.Ids.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("Parent", string.Join(",", request.Ids));

        foreach (var p in entities)
        {
            p.IsDeleted = true;
            p.DeletedAt = DateTime.UtcNow;
            p.UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success(true);
    }
}