using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Commands.User.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateUserCommandHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<UserResponse>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var user = await _dbContext.Set<UserEntity>()
            .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, cancellationToken);
        if (user == null)
            return Error.NotFound("User", request.Id.ToString());

        user.Name = request.Name;
        user.Phone = request.Phone;
        user.TenantId = request.TenantId;
        user.BranchId = request.BranchId;
        user.Status = request.Status ?? user.Status;
        user.UpdatedAt = DateTime.UtcNow;

        // SaveChanges handled by TransactionBehavior

        return Result.Success(user.Adapt<UserResponse>());
    }
}