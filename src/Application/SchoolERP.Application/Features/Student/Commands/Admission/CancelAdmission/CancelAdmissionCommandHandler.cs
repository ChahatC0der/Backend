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
using AdmissionEntity = SchoolERP.Domain.Student.Admission.Entities.Admission;

namespace SchoolERP.Application.Features.Student.Commands.Admission.CancelAdmission;

public class CancelAdmissionCommandHandler : IRequestHandler<CancelAdmissionCommand, Result<AdmissionResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CancelAdmissionCommandHandler(
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

    public async Task<Result<AdmissionResponse>> Handle(CancelAdmissionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var entityResult = await _dbContext.GetEntityAsync<AdmissionEntity>(
            a => a.Id == request.Id && a.BranchId == branchId && !a.IsDeleted,
            "Admission", request.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var admission = entityResult.Value;

        if (admission.Status == "enrolled")
            return Error.Conflict("Cannot cancel an enrolled admission.");

        if (admission.Status == "cancelled")
            return Error.Conflict("Admission is already cancelled.");

        var oldStatus = admission.Status;
        admission.Status = "cancelled";
        admission.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "Admission",
            Action = "cancel",
            OldValues = System.Text.Json.JsonSerializer.Serialize(new { Status = oldStatus }),
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { Status = "cancelled", Reason = request.Reason }),
            CreatedAt = DateTime.UtcNow
        });

        var response = admission.Adapt<AdmissionResponse>();
        return Result.Success(await FillComputedAsync(response, cancellationToken));
    }

    private async Task<AdmissionResponse> FillComputedAsync(AdmissionResponse r, CancellationToken ct)
    {
        var ayName = await _dbContext.Set<AcademicYearEntity>().AsNoTracking()
            .Where(a => a.Id == r.AcademicYearId).Select(a => a.Name).FirstOrDefaultAsync(ct);
        var className = await _dbContext.Set<ClassEntity>().AsNoTracking()
            .Where(c => c.Id == r.ClassId).Select(c => c.Name).FirstOrDefaultAsync(ct);
        var sectionName = await _dbContext.Set<SectionEntity>().AsNoTracking()
            .Where(s => s.Id == r.SectionId).Select(s => s.Name).FirstOrDefaultAsync(ct);
        return r with { AcademicYearName = ayName, ClassName = className, SectionName = sectionName };
    }
}
