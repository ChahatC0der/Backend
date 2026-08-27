using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Commands.User.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateUserCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserResponse>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Email uniqueness check
        var exists = await _dbContext.Set<UserEntity>()
            .AnyAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken);
        if (exists)
            return Error.Conflict($"User with email '{request.Email}' already exists.");

        var user = request.Adapt<UserEntity>();
        // TODO: Replace with proper password hashing (e.g., BCrypt, Identity PasswordHasher) later
        user.PasswordHash = request.Password; // temporary plain text
        user.PermissionsVersion = 1;
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        _dbContext.Set<UserEntity>().Add(user);
        // SaveChanges handled by TransactionBehavior

        return Result.Success(user.Adapt<UserResponse>());
    }
}