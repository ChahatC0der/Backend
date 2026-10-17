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
using EnquiryEntity = SchoolERP.Domain.Student.Enquiry.Entities.Enquiry;
using AdmissionEntity = SchoolERP.Domain.Student.Admission.Entities.Admission;

namespace SchoolERP.Application.Features.Student.Commands.Admission.CreateAdmission;

public class CreateAdmissionCommandHandler : IRequestHandler<CreateAdmissionCommand, Result<AdmissionResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreateAdmissionCommandHandler(
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

    public async Task<Result<AdmissionResponse>> Handle(CreateAdmissionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        // Validate academic year, class, section
        var ayErr = await _dbContext.EnsureEntityExistsAsync<AcademicYearEntity>(request.AcademicYearId, cancellationToken);
        if (ayErr != null) return Error.NotFound("AcademicYear", request.AcademicYearId.ToString());

        var classErr = await _dbContext.EnsureEntityExistsAsync<ClassEntity>(request.ClassId, cancellationToken);
        if (classErr != null) return Error.NotFound("Class", request.ClassId.ToString());

        var secErr = await _dbContext.EnsureEntityExistsAsync<SectionEntity>(request.SectionId, cancellationToken);
        if (secErr != null) return Error.NotFound("Section", request.SectionId.ToString());

        // Validate Registration if provided
        if (request.RegistrationId.HasValue)
        {
            var regExists = await _dbContext.EnsureUniqueAsync<RegistrationEntity>(
                r => r.Id == request.RegistrationId.Value && r.BranchId == branchId && !r.IsDeleted,
                "Registration not found.", cancellationToken);
            if (regExists != null) return Error.NotFound("Registration", request.RegistrationId.Value.ToString());

            var dupCheck = await _dbContext.EnsureUniqueAsync<AdmissionEntity>(
                a => a.RegistrationId == request.RegistrationId.Value && !a.IsDeleted,
                "Admission already exists for this registration.", cancellationToken);
            if (dupCheck != null) return dupCheck;
        }

        // Validate Enquiry if provided
        if (request.EnquiryId.HasValue)
        {
            var enqExists = await _dbContext.EnsureUniqueAsync<EnquiryEntity>(
                e => e.Id == request.EnquiryId.Value && e.BranchId == branchId && !e.IsDeleted,
                "Enquiry not found.", cancellationToken);
            if (enqExists != null) return Error.NotFound("Enquiry", request.EnquiryId.Value.ToString());

            var dupCheck = await _dbContext.EnsureUniqueAsync<AdmissionEntity>(
                a => a.EnquiryId == request.EnquiryId.Value && !a.IsDeleted,
                "Admission already exists for this enquiry.", cancellationToken);
            if (dupCheck != null) return dupCheck;
        }

        var userId = _currentUserService.GetUserId() ?? 0;

        var admission = request.Adapt<AdmissionEntity>();
        admission.TenantId = tenantId;
        admission.AdmissionDate = request.AdmissionDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        admission.Status = "in_progress";
        admission.CreatedBy = userId;

        _dbContext.Set<AdmissionEntity>().Add(admission);

        //_dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = userId,
        //    Resource = "Admission",
        //    Action = "create",
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new
        //    {
        //        admission.RegistrationId,
        //        admission.EnquiryId,
        //        admission.ClassId,
        //        admission.SectionId
        //    }),
        //    CreatedAt = DateTime.UtcNow
        //});

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

        string? regNumber = null;
        if (r.RegistrationId.HasValue)
            regNumber = await _dbContext.Set<RegistrationEntity>().AsNoTracking()
                .Where(x => x.Id == r.RegistrationId.Value).Select(x => x.RegistrationNumber).FirstOrDefaultAsync(ct);

        return r with { AcademicYearName = ayName, ClassName = className, SectionName = sectionName, RegistrationNumber = regNumber };
    }
}
