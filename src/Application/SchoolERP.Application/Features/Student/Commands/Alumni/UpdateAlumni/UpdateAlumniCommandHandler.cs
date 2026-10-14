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

namespace SchoolERP.Application.Features.Student.Commands.Alumni.UpdateAlumni;

public class UpdateAlumniCommandHandler : IRequestHandler<UpdateAlumniCommand, Result<AlumniResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateAlumniCommandHandler(
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

    public async Task<Result<AlumniResponse>> Handle(UpdateAlumniCommand command, CancellationToken cancellationToken)
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
            alumni.FirstName,
            alumni.CurrentStatus,
            alumni.Occupation,
            alumni.ContactMobile
        });

        alumni.FirstName = request.FirstName;
        alumni.LastName = request.LastName;
        alumni.DateOfBirth = request.DateOfBirth;
        alumni.Gender = request.Gender;
        alumni.GraduationDate = request.GraduationDate;
        alumni.LastClassId = request.LastClassId;
        alumni.PassingAcademicYearId = request.PassingAcademicYearId;
        alumni.FinalPercentage = request.FinalPercentage;
        alumni.CurrentStatus = request.CurrentStatus;
        alumni.Occupation = request.Occupation;
        alumni.CompanyName = request.CompanyName;
        alumni.Industry = request.Industry;
        alumni.HigherEducationInstitution = request.HigherEducationInstitution;
        alumni.HigherEducationCourse = request.HigherEducationCourse;
        alumni.HigherEducationYear = request.HigherEducationYear;
        alumni.ContactEmail = request.ContactEmail;
        alumni.ContactMobile = request.ContactMobile;
        alumni.Address = request.Address;
        alumni.SocialLinks = request.SocialLinks;
        alumni.PhotoUrl = request.PhotoUrl;
        alumni.PrivacySettings = request.PrivacySettings;
        alumni.CommunicationPreferences = request.CommunicationPreferences;
        alumni.Tags = request.Tags;
        alumni.IsDeceased = request.IsDeceased;
        alumni.IsArchived = request.IsArchived;
        alumni.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "Alumni",
            Action = "update",
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                alumni.FirstName,
                alumni.CurrentStatus,
                alumni.Occupation,
                alumni.ContactMobile
            }),
            CreatedAt = DateTime.UtcNow
        });

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
