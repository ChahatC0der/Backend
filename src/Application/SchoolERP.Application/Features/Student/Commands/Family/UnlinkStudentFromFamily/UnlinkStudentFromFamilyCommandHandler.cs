using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using FamilyEntity = SchoolERP.Domain.Student.Entities.Family;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Student.Commands.Family.UnlinkStudentFromFamily;

public class UnlinkStudentFromFamilyCommandHandler : IRequestHandler<UnlinkStudentFromFamilyCommand, Result<FamilyResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UnlinkStudentFromFamilyCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentBranchService branchService,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<FamilyResponse>> Handle(UnlinkStudentFromFamilyCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var family = await _dbContext.Set<FamilyEntity>()
            .Include(f => f.Students)
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId && f.BranchId == branchId && !f.IsDeleted, cancellationToken);
        if (family == null) return Error.NotFound("Family", request.FamilyId.ToString());

        var student = await _dbContext.Set<StudentEntity>()
            .FirstOrDefaultAsync(s => s.Id == request.StudentId && s.BranchId == branchId && !s.IsDeleted, cancellationToken);
        if (student == null) return Error.NotFound("Student", request.StudentId.ToString());

        if (student.FamilyId != family.Id)
            return Error.Conflict("Student is not linked to this family.");

        student.FamilyId = null;
        student.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        //_dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = userId,
        //    Resource = "Family",
        //    Action = "unlink_student",
        //    OldValues = System.Text.Json.JsonSerializer.Serialize(new { FamilyId = family.Id, StudentId = student.Id }),
        //    CreatedAt = DateTime.UtcNow
        //});

        // Remove from navigation to reflect count
        family.Students.Remove(student);

        return Result.Success(family.Adapt<FamilyResponse>());
    }
}
