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

namespace SchoolERP.Application.Features.Student.Commands.Registration.UpdateRegistration;

public class UpdateRegistrationCommandHandler : IRequestHandler<UpdateRegistrationCommand, Result<RegistrationResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateRegistrationCommandHandler(
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

    public async Task<Result<RegistrationResponse>> Handle(UpdateRegistrationCommand command, CancellationToken cancellationToken)
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

        if (request.SectionId.HasValue)
        {
            var secErr = await _dbContext.EnsureEntityExistsAsync<SectionEntity>(request.SectionId.Value, cancellationToken);
            if (secErr != null) return Error.NotFound("Section", request.SectionId.Value.ToString());
        }

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            registration.StudentFirstName,
            registration.ParentPrimaryMobile,
            registration.ClassId,
            registration.SectionId
        });

        registration.AcademicYearId = request.AcademicYearId;
        registration.ClassId = request.ClassId;
        registration.SectionId = request.SectionId;
        registration.StudentFirstName = request.StudentFirstName;
        registration.StudentLastName = request.StudentLastName;
        registration.DateOfBirth = request.DateOfBirth;
        registration.ParentPrimaryMobile = request.ParentPrimaryMobile;
        registration.Gender = request.Gender;
        registration.Birthplace = request.Birthplace;
        registration.Nationality = request.Nationality;
        registration.MotherTongue = request.MotherTongue;
        registration.Religion = request.Religion;
        registration.AadharNumber = request.AadharNumber;
        registration.FatherName = request.FatherName;
        registration.FatherMobile = request.FatherMobile;
        registration.FatherEmail = request.FatherEmail;
        registration.FatherOccupation = request.FatherOccupation;
        registration.MotherName = request.MotherName;
        registration.MotherMobile = request.MotherMobile;
        registration.MotherEmail = request.MotherEmail;
        registration.MotherOccupation = request.MotherOccupation;
        registration.GuardianName = request.GuardianName;
        registration.GuardianMobile = request.GuardianMobile;
        registration.GuardianEmail = request.GuardianEmail;
        registration.GuardianOccupation = request.GuardianOccupation;
        registration.ParentPrimaryEmail = request.ParentPrimaryEmail;
        registration.PresentAddress = request.PresentAddress;
        registration.PermanentAddress = request.PermanentAddress;
        registration.IsPermanentSame = request.IsPermanentSame;
        registration.PreviousSchool = request.PreviousSchool;
        registration.PreviousBoard = request.PreviousBoard;
        registration.PreviousPercentage = request.PreviousPercentage;
        registration.PreviousYearPassing = request.PreviousYearPassing;
        registration.TransferCertificateUrl = request.TransferCertificateUrl;
        registration.Source = request.Source;
        registration.MedicalConditions = request.MedicalConditions;
        registration.CustomFields = request.CustomFields;
        registration.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        //_dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = userId,
        //    Resource = "Registration",
        //    Action = "update",
        //    OldValues = oldValues,
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new
        //    {
        //        registration.StudentFirstName,
        //        registration.ParentPrimaryMobile,
        //        registration.ClassId,
        //        registration.SectionId
        //    }),
        //    CreatedAt = DateTime.UtcNow
        //});

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
