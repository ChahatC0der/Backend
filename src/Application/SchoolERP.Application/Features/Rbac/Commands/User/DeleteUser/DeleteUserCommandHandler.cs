using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Commands.User.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteUserCommandHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<bool>> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Set<UserEntity>()
            .FirstOrDefaultAsync(u => u.Id == command.UserId && !u.IsDeleted, cancellationToken);
        if (user == null)
            return Error.NotFound("User", command.UserId.ToString());

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        // SaveChanges handled by TransactionBehavior

        return Result.Success(true);
    }
}