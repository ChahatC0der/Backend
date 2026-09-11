using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using ParentEntity = SchoolERP.Domain.Student.Entities.Parent;

namespace SchoolERP.Application.Features.Student.Commands.Parent.BulkPatchParent;

public class BulkPatchParentCommandHandler : IRequestHandler<BulkPatchParentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public BulkPatchParentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(BulkPatchParentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var entities = await _dbContext.Set<ParentEntity>()
            .Where(p => request.Ids.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("Parent", string.Join(",", request.Ids));

        foreach (var p in entities)
        {
            request.Occupation.PatchIfProvided(v => p.Occupation = v);
            if (request.IsPrimary.HasValue) p.IsPrimary = request.IsPrimary.Value;
            p.UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success(true);
    }
}
