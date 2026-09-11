using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using ParentEntity = SchoolERP.Domain.Student.Entities.Parent;

namespace SchoolERP.Application.Features.Student.Commands.Parent.BulkUpdateParent;

public class BulkUpdateParentCommandHandler : IRequestHandler<BulkUpdateParentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public BulkUpdateParentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(BulkUpdateParentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var entities = await _dbContext.Set<ParentEntity>()
            .Where(p => request.Ids.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("Parent", string.Join(",", request.Ids));

        foreach (var p in entities)
        {
            if (request.Occupation != null) p.Occupation = request.Occupation;
            p.IsPrimary = request.IsPrimary;
            p.UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success(true);
    }
}
