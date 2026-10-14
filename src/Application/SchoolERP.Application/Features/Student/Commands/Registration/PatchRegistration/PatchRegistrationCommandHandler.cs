using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;

namespace SchoolERP.Application.Features.Student.Commands.Registration.PatchRegistration;

public class PatchRegistrationCommandHandler : IRequestHandler<PatchRegistrationCommand, Result<RegistrationResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public PatchRegistrationCommandHandler(
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

    public async Task<Result<RegistrationResponse>> Handle(PatchRegistrationCommand command, CancellationToken cancellationToken)
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

        var registration = entityResult.Value;

        if (registration.Status != "draft" && registration.Status != "submitted")
            return Error.Conflict($"Cannot edit registration in '{registration.Status}' status.");

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            registration.StudentFirstName,
            registration.ParentPrimaryMobile,
            registration.ClassId,
            registration.SectionId
        });

        // Strings — PatchIfProvided
        request.StudentFirstName.PatchIfProvided(v => registration.StudentFirstName = v);
        request.StudentLastName.PatchIfProvided(v => registration.StudentLastName = v);
        request.ParentPrimaryMobile.PatchIfProvided(v => registration.ParentPrimaryMobile = v);
        request.Gender.PatchIfProvided(v => registration.Gender = v);
        request.Birthplace.PatchIfProvided(v => registration.Birthplace = v);
        request.Nationality.PatchIfProvided(v => registration.Nationality = v);
        request.MotherTongue.PatchIfProvided(v => registration.MotherTongue = v);
        request.Religion.PatchIfProvided(v => registration.Religion = v);
        request.AadharNumber.PatchIfProvided(v => registration.AadharNumber = v);
        request.FatherName.PatchIfProvided(v => registration.FatherName = v);
        request.FatherMobile.PatchIfProvided(v => registration.FatherMobile = v);
        request.FatherEmail.PatchIfProvided(v => registration.FatherEmail = v);
        request.FatherOccupation.PatchIfProvided(v => registration.FatherOccupation = v);
        request.MotherName.PatchIfProvided(v => registration.MotherName = v);
        request.MotherMobile.PatchIfProvided(v => registration.MotherMobile = v);
        request.MotherEmail.PatchIfProvided(v => registration.MotherEmail = v);
        request.MotherOccupation.PatchIfProvided(v => registration.MotherOccupation = v);
        request.GuardianName.PatchIfProvided(v => registration.GuardianName = v);
        request.GuardianMobile.PatchIfProvided(v => registration.GuardianMobile = v);
        request.GuardianEmail.PatchIfProvided(v => registration.GuardianEmail = v);
        request.GuardianOccupation.PatchIfProvided(v => registration.GuardianOccupation = v);
        request.ParentPrimaryEmail.PatchIfProvided(v => registration.ParentPrimaryEmail = v);
        request.PresentAddress.PatchIfProvided(v => registration.PresentAddress = v);
        request.PermanentAddress.PatchIfProvided(v => registration.PermanentAddress = v);
        request.PreviousSchool.PatchIfProvided(v => registration.PreviousSchool = v);
        request.PreviousBoard.PatchIfProvided(v => registration.PreviousBoard = v);
        request.TransferCertificateUrl.PatchIfProvided(v => registration.TransferCertificateUrl = v);
        request.Source.PatchIfProvided(v => registration.Source = v);
        request.MedicalConditions.PatchIfProvided(v => registration.MedicalConditions = v);
        request.CustomFields.PatchIfProvided(v => registration.CustomFields = v);

        // Nullable ID / Date / Bool / Decimal
        if (request.AcademicYearId.HasValue) registration.AcademicYearId = request.AcademicYearId.Value;
        if (request.ClassId.HasValue) registration.ClassId = request.ClassId.Value;
        if (request.SectionId.HasValue) registration.SectionId = request.SectionId.Value;
        if (request.DateOfBirth.HasValue) registration.DateOfBirth = request.DateOfBirth.Value;
        if (request.IsPermanentSame.HasValue) registration.IsPermanentSame = request.IsPermanentSame.Value;
        if (request.PreviousPercentage.HasValue) registration.PreviousPercentage = request.PreviousPercentage.Value;
        if (request.PreviousYearPassing.HasValue) registration.PreviousYearPassing = request.PreviousYearPassing.Value;

        registration.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "Registration",
            Action = "patch",
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                registration.StudentFirstName,
                registration.ParentPrimaryMobile,
                registration.ClassId,
                registration.SectionId
            }),
            CreatedAt = DateTime.UtcNow
        });

        var response = registration.Adapt<RegistrationResponse>();
        return Result.Success(await FillComputedAsync(response, cancellationToken));
    }

    private async Task<RegistrationResponse> FillComputedAsync(RegistrationResponse r, CancellationToken ct)
    {
        var className = await _dbContext.Set<ClassEntity>().AsNoTracking()
            .Where(c => c.Id == r.ClassId).Select(c => c.Name).FirstOrDefaultAsync(ct);

        string? sectionName = null;
        if (r.SectionId.HasValue)
            sectionName = await _dbContext.Set<SectionEntity>().AsNoTracking()
                .Where(s => s.Id == r.SectionId.Value).Select(s => s.Name).FirstOrDefaultAsync(ct);

        return r with { ClassName = className, SectionName = sectionName };
    }
}
