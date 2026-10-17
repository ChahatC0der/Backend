using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;

namespace SchoolERP.Application.Features.Student.Commands.Registration.ConvertRegistrationToAdmission;

public class ConvertRegistrationToAdmissionCommandHandler : IRequestHandler<ConvertRegistrationToAdmissionCommand, Result<RegistrationResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public ConvertRegistrationToAdmissionCommandHandler(
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

    public async Task<Result<RegistrationResponse>> Handle(ConvertRegistrationToAdmissionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var entityResult = await _dbContext.GetEntityAsync<RegistrationEntity>(
            r => r.Id == request.Id && r.BranchId == branchId.Value && !r.IsDeleted,
            "Registration", request.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var reg = entityResult.Value;

        if (reg.Status != "approved")
            return Error.Conflict($"Cannot convert registration in '{reg.Status}' status. Must be approved.");

        var ayErr = await _dbContext.EnsureEntityExistsAsync<AcademicYearEntity>(request.AcademicYearId, cancellationToken);
        if (ayErr != null) return Error.NotFound("AcademicYear", request.AcademicYearId.ToString());

        var classErr = await _dbContext.EnsureEntityExistsAsync<ClassEntity>(request.ClassId, cancellationToken);
        if (classErr != null) return Error.NotFound("Class", request.ClassId.ToString());

        var secErr = await _dbContext.EnsureEntityExistsAsync<SectionEntity>(request.SectionId, cancellationToken);
        if (secErr != null) return Error.NotFound("Section", request.SectionId.ToString());

        var userId = _currentUserService.GetUserId() ?? 0;
        var oldStatus = reg.Status;

        reg.Status = "converted";
        reg.UpdatedAt = DateTime.UtcNow;

        // TODO: Create Admission + Student record — link reg.ConvertedToStudentId

        //_dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = userId,
        //    Resource = "Registration",
        //    Action = "convert",
        //    OldValues = System.Text.Json.JsonSerializer.Serialize(new { Status = oldStatus }),
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new
        //    {
        //        Status = "converted",
        //        request.AcademicYearId,
        //        request.ClassId,
        //        request.SectionId
        //    }),
        //    CreatedAt = DateTime.UtcNow
        //});

        var response = reg.Adapt<RegistrationResponse>();
        return Result.Success(await FillComputedAsync(response, cancellationToken));
    }

    private async Task<RegistrationResponse> FillComputedAsync(RegistrationResponse r, CancellationToken ct)
    {
        var className = await _dbContext.Set<ClassEntity>().AsNoTracking().Where(c => c.Id == r.ClassId).Select(c => c.Name).FirstOrDefaultAsync(ct);
        string? sectionName = null;
        if (r.SectionId.HasValue)
            sectionName = await _dbContext.Set<SectionEntity>().AsNoTracking().Where(s => s.Id == r.SectionId.Value).Select(s => s.Name).FirstOrDefaultAsync(ct);
        return r with { ClassName = className, SectionName = sectionName };
    }
}
