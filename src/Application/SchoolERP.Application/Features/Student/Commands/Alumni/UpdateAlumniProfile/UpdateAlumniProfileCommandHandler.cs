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
using AlumniEntity = SchoolERP.Domain.Student.Alumni.Entities.Alumni;

namespace SchoolERP.Application.Features.Student.Commands.Alumni.UpdateAlumniProfile;

public class UpdateAlumniProfileCommandHandler : IRequestHandler<UpdateAlumniProfileCommand, Result<AlumniResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateAlumniProfileCommandHandler(
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

    public async Task<Result<AlumniResponse>> Handle(UpdateAlumniProfileCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var entityResult = await _dbContext.GetEntityAsync<AlumniEntity>(
            a => a.Id == request.Id && a.BranchId == branchId && !a.IsDeleted,
            "Alumni", request.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var alumni = entityResult.Value;

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            alumni.Occupation,
            alumni.CompanyName,
            alumni.ContactMobile
        });

        request.Occupation.PatchIfProvided(v => alumni.Occupation = v);
        request.CompanyName.PatchIfProvided(v => alumni.CompanyName = v);
        request.Industry.PatchIfProvided(v => alumni.Industry = v);
        request.HigherEducationInstitution.PatchIfProvided(v => alumni.HigherEducationInstitution = v);
        request.HigherEducationCourse.PatchIfProvided(v => alumni.HigherEducationCourse = v);
        request.ContactEmail.PatchIfProvided(v => alumni.ContactEmail = v);
        request.ContactMobile.PatchIfProvided(v => alumni.ContactMobile = v);
        request.Address.PatchIfProvided(v => alumni.Address = v);
        request.SocialLinks.PatchIfProvided(v => alumni.SocialLinks = v);
        request.PhotoUrl.PatchIfProvided(v => alumni.PhotoUrl = v);
        request.PrivacySettings.PatchIfProvided(v => alumni.PrivacySettings = v);
        request.CommunicationPreferences.PatchIfProvided(v => alumni.CommunicationPreferences = v);

        if (request.HigherEducationYear.HasValue) alumni.HigherEducationYear = request.HigherEducationYear.Value;

        alumni.LastInteractionDate = DateOnly.FromDateTime(DateTime.UtcNow);
        alumni.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        //_dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = userId,
        //    Resource = "Alumni",
        //    Action = "profile_update",
        //    OldValues = oldValues,
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new
        //    {
        //        alumni.Occupation,
        //        alumni.CompanyName,
        //        alumni.ContactMobile
        //    }),
        //    CreatedAt = DateTime.UtcNow
        //});

        var response = alumni.Adapt<AlumniResponse>();
        return Result.Success(await FillComputedAsync(response, cancellationToken));
    }

    private async Task<AlumniResponse> FillComputedAsync(AlumniResponse r, CancellationToken ct)
    {
        var className = await _dbContext.Set<ClassEntity>().AsNoTracking()
            .Where(c => c.Id == r.LastClassId).Select(c => c.Name).FirstOrDefaultAsync(ct);
        var ayName = await _dbContext.Set<AcademicYearEntity>().AsNoTracking()
            .Where(a => a.Id == r.PassingAcademicYearId).Select(a => a.Name).FirstOrDefaultAsync(ct);
        return r with { LastClassName = className, PassingAcademicYearName = ayName };
    }
}
