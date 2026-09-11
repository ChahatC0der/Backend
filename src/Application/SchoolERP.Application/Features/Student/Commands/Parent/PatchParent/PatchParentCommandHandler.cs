using Mapster;
using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using ParentEntity = SchoolERP.Domain.Student.Entities.Parent;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;

namespace SchoolERP.Application.Features.Student.Commands.Parent.PatchParent;

public class PatchParentCommandHandler : IRequestHandler<PatchParentCommand, Result<ParentResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public PatchParentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ParentResponse>> Handle(PatchParentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var entityResult = await _dbContext.GetEntityAsync<ParentEntity>(
            p => p.Id == request.Id && !p.IsDeleted,
            "Parent", request.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var parent = entityResult.Value;

        if (request.UserId.HasValue)
        {
            var userExists = await _dbContext.EnsureEntityExistsAsync<UserEntity>(request.UserId.Value, cancellationToken);
            if (userExists != null) return Error.NotFound("User", request.UserId.Value.ToString());
            parent.UserId = request.UserId;
        }

        request.ParentType.PatchIfProvided(v => parent.ParentType = v);
        request.FirstName.PatchIfProvided(v => parent.FirstName = v);
        request.LastName.PatchIfProvided(v => parent.LastName = v);
        request.Email.PatchIfProvided(v => parent.Email = v);
        request.Mobile.PatchIfProvided(v => parent.Mobile = v);
        request.Occupation.PatchIfProvided(v => parent.Occupation = v);
        if (request.IsPrimary.HasValue) parent.IsPrimary = request.IsPrimary.Value;

        parent.UpdatedAt = DateTime.UtcNow;

        return Result.Success(parent.Adapt<ParentResponse>());
    }
}
