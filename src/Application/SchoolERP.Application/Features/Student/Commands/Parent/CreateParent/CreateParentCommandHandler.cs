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

namespace SchoolERP.Application.Features.Student.Commands.Parent.CreateParent;

public class CreateParentCommandHandler : IRequestHandler<CreateParentCommand, Result<ParentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;

    public CreateParentCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentBranchService branchService,
        ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
        _tenantService = tenantService;
    }

    public async Task<Result<ParentResponse>> Handle(CreateParentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;
        var tenantId = _tenantService.GetTenantId();

        var student = await _dbContext.Set<StudentEntity>()
            .FirstOrDefaultAsync(s => s.Id == request.StudentId && s.BranchId == branchId && !s.IsDeleted, cancellationToken);
        if (student == null) return Error.NotFound("Student", request.StudentId.ToString());

        if (request.UserId.HasValue)
        {
            var userExists = await _dbContext.EnsureEntityExistsAsync<UserEntity>(request.UserId.Value, cancellationToken);
            if (userExists != null) return Error.NotFound("User", request.UserId.Value.ToString());
        }

        var parent = request.Adapt<ParentEntity>();
        parent.TenantId = tenantId;

        _dbContext.Set<ParentEntity>().Add(parent);

        return Result.Success(parent.Adapt<ParentResponse>());
    }
}
