using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using ParentEntity = SchoolERP.Domain.Student.Entities.Parent;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;

namespace SchoolERP.Application.Features.Student.Commands.Parent.UpdateParent;

public class UpdateParentCommandHandler : IRequestHandler<UpdateParentCommand, Result<ParentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public UpdateParentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branch_service = branchService;
    }

    public async Task<Result<ParentResponse>> Handle(UpdateParentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branch_service.GetBranchId() ?? Guid.Empty;

        var entityResult = await _dbContext.GetEntityAsync<ParentEntity>(
            p => p.Id == request.Id && !p.IsDeleted,
            "Parent", request.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var parent = entityResult.Value;

        var student = await _dbContext.Set<StudentEntity>()
            .FirstOrDefaultAsync(s => s.Id == request.StudentId && s.BranchId == branchId && !s.IsDeleted, cancellationToken);
        if (student == null) return Error.NotFound("Student", request.StudentId.ToString());

        if (request.UserId.HasValue)
        {
            var userExists = await _dbContext.EnsureEntityExistsAsync<UserEntity>(request.UserId.Value, cancellationToken);
            if (userExists != null) return Error.NotFound("User", request.UserId.Value.ToString());
        }

        parent.StudentId = request.StudentId;
        parent.ParentType = request.ParentType;
        parent.FirstName = request.FirstName;
        parent.LastName = request.LastName;
        parent.Email = request.Email;
        parent.Mobile = request.Mobile;
        parent.Occupation = request.Occupation;
        parent.IsPrimary = request.IsPrimary;
        parent.UserId = request.UserId;
        parent.UpdatedAt = DateTime.UtcNow;

        return Result.Success(parent.Adapt<ParentResponse>());
    }
}
