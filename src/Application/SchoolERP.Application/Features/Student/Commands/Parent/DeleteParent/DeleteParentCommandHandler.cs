using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using ParentEntity = SchoolERP.Domain.Student.Entities.Parent;

namespace SchoolERP.Application.Features.Student.Commands.Parent.DeleteParent;

public class DeleteParentCommandHandler : IRequestHandler<DeleteParentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteParentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(DeleteParentCommand command, CancellationToken cancellationToken)
    {
        var entityResult = await _dbContext.GetEntityAsync<ParentEntity>(
            p => p.Id == command.Id && !p.IsDeleted,
            "Parent", command.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var parent = entityResult.Value;
        parent.IsDeleted = true;
        parent.DeletedAt = DateTime.UtcNow;
        parent.UpdatedAt = DateTime.UtcNow;

        return Result.Success(true);
    }
}
