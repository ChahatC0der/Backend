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
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;
using ParentEntity = SchoolERP.Domain.Student.Entities.Parent;
using FamilyEntity = SchoolERP.Domain.Student.Entities.Family;

namespace SchoolERP.Application.Features.Student.Commands.Admission.EnrollNow;

public class EnrollNowCommandHandler : IRequestHandler<EnrollNowCommand, Result<AdmissionResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEnrollmentIdGenerator _enrollmentIdGenerator;

    public EnrollNowCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentBranchService branchService,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService,
        IEnrollmentIdGenerator enrollmentIdGenerator)
    {
        _dbContext = dbContext;
        _branchService = branchService;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
        _enrollmentIdGenerator = enrollmentIdGenerator;
    }

    public async Task<Result<AdmissionResponse>> Handle(EnrollNowCommand command, CancellationToken cancellationToken)
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
            return Error.Conflict("Admission is already enrolled.");
        if (admission.Status == "cancelled")
            return Error.Conflict("Cannot enroll a cancelled admission.");
        if (!admission.FirstFeePaid)
            return Error.Conflict("First fee must be collected or waived before enrollment.");

        var userId = _currentUserService.GetUserId() ?? 0;

        // Load source data
        RegistrationEntity? registration = null;
        if (admission.RegistrationId.HasValue)
        {
            registration = await _dbContext.Set<RegistrationEntity>()
                .FirstOrDefaultAsync(r => r.Id == admission.RegistrationId.Value, cancellationToken);
        }

        // Determine student data (from registration OR request)
        string firstName, parentPrimaryMobile;
        string? lastName = null, gender = null, personalMobile = null, personalEmail = null, address = null;
        DateOnly dateOfBirth = default;

        if (registration != null)
        {
            firstName = registration.StudentFirstName;
            lastName = registration.StudentLastName;
            dateOfBirth = registration.DateOfBirth;
            gender = registration.Gender;
            personalMobile = registration.ParentPrimaryMobile;
            personalEmail = registration.ParentPrimaryEmail;
            address = registration.PresentAddress;
            parentPrimaryMobile = registration.ParentPrimaryMobile;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.StudentFirstName))
                return Error.Validation("Student first name is required for direct admission.");
            if (!request.DateOfBirth.HasValue)
                return Error.Validation("Date of birth is required for direct admission.");
            if (string.IsNullOrWhiteSpace(request.ParentPrimaryMobile))
                return Error.Validation("Parent mobile is required for direct admission.");

            firstName = request.StudentFirstName;
            lastName = request.StudentLastName;
            dateOfBirth = request.DateOfBirth.Value;
            gender = request.Gender;
            personalMobile = request.PersonalMobile;
            personalEmail = request.PersonalEmail;
            address = request.Address;
            parentPrimaryMobile = request.ParentPrimaryMobile;
        }

        // 1) Generate EnrollmentId
        var enrollmentId = await _enrollmentIdGenerator.GenerateAsync(branchId.Value, cancellationToken);

        // 2) Create Student
        var student = new StudentEntity
        {
            TenantId = tenantId,
            BranchId = branchId.Value,
            EnrollmentId = enrollmentId,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            PersonalMobile = personalMobile,
            PersonalEmail = personalEmail,
            Address = address,
            AdmissionDate = admission.AdmissionDate,
            RollNumber = request.RollNumber ?? admission.RollNumber,
            Status = "enrolled",
            FamilyId = request.FamilyId
        };
        _dbContext.Set<StudentEntity>().Add(student);

        // 3) Family: if not linked, try to match by head mobile (same branch)
        if (!student.FamilyId.HasValue && !string.IsNullOrWhiteSpace(parentPrimaryMobile))
        {
            var existingFamily = await _dbContext.Set<FamilyEntity>()
                .FirstOrDefaultAsync(f => f.BranchId == branchId.Value && f.HeadMobile == parentPrimaryMobile && !f.IsDeleted, cancellationToken);

            if (existingFamily != null)
            {
                student.FamilyId = existingFamily.Id;
            }
            else
            {
                var family = new FamilyEntity
                {
                    TenantId = tenantId,
                    BranchId = branchId.Value,
                    HeadName = registration != null ? registration.FatherName ?? registration.GuardianName : request.FatherName ?? request.GuardianName,
                    HeadMobile = parentPrimaryMobile,
                    HeadEmail = registration?.ParentPrimaryEmail ?? request.ParentPrimaryEmail
                };
                _dbContext.Set<FamilyEntity>().Add(family);
                student.Family = family;
            }
        }

        // 4) Create StudentEnrollment (current placement)
        var studentEnrollment = new StudentEnrollmentEntity
        {
            Student = student,
            AcademicYearId = admission.AcademicYearId,
            ClassId = admission.ClassId,
            SectionId = admission.SectionId,
            RollNumber = request.RollNumber ?? admission.RollNumber,
            IsCurrent = true,
            EnrolledAt = request.JoiningDate ?? DateOnly.FromDateTime(DateTime.UtcNow)
        };
        _dbContext.Set<StudentEnrollmentEntity>().Add(studentEnrollment);

        // 5) Create Parents (from registration if available, else from request)
        if (registration != null)
        {
            if (!string.IsNullOrWhiteSpace(registration.FatherName))
            {
                _dbContext.Set<ParentEntity>().Add(new ParentEntity
                {
                    TenantId = tenantId,
                    Student = student,
                    ParentType = "father",
                    FirstName = registration.FatherName,
                    Mobile = registration.FatherMobile,
                    Email = registration.FatherEmail,
                    Occupation = registration.FatherOccupation,
                    IsPrimary = true
                });
            }
            if (!string.IsNullOrWhiteSpace(registration.MotherName))
            {
                _dbContext.Set<ParentEntity>().Add(new ParentEntity
                {
                    TenantId = tenantId,
                    Student = student,
                    ParentType = "mother",
                    FirstName = registration.MotherName,
                    Mobile = registration.MotherMobile,
                    Email = registration.MotherEmail,
                    Occupation = registration.MotherOccupation,
                    IsPrimary = false
                });
            }
            if (!string.IsNullOrWhiteSpace(registration.GuardianName))
            {
                _dbContext.Set<ParentEntity>().Add(new ParentEntity
                {
                    TenantId = tenantId,
                    Student = student,
                    ParentType = "guardian",
                    FirstName = registration.GuardianName,
                    Mobile = registration.GuardianMobile,
                    Email = registration.GuardianEmail,
                    Occupation = registration.GuardianOccupation,
                    IsPrimary = false
                });
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(request.FatherName))
            {
                _dbContext.Set<ParentEntity>().Add(new ParentEntity
                {
                    TenantId = tenantId,
                    Student = student,
                    ParentType = "father",
                    FirstName = request.FatherName,
                    Mobile = request.FatherMobile,
                    IsPrimary = true
                });
            }
            if (!string.IsNullOrWhiteSpace(request.MotherName))
            {
                _dbContext.Set<ParentEntity>().Add(new ParentEntity
                {
                    TenantId = tenantId,
                    Student = student,
                    ParentType = "mother",
                    FirstName = request.MotherName,
                    Mobile = request.MotherMobile,
                    IsPrimary = false
                });
            }
            if (!string.IsNullOrWhiteSpace(request.GuardianName))
            {
                _dbContext.Set<ParentEntity>().Add(new ParentEntity
                {
                    TenantId = tenantId,
                    Student = student,
                    ParentType = "guardian",
                    FirstName = request.GuardianName,
                    Mobile = request.GuardianMobile,
                    IsPrimary = false
                });
            }
        }

        // 6) Update Admission
        admission.StudentId = student.Id;
        admission.Status = "enrolled";
        admission.EnrolledAt = DateTime.UtcNow;
        admission.EnrolledBy = userId;
        admission.RollNumber = studentEnrollment.RollNumber;
        if (request.JoiningDate.HasValue) admission.JoiningDate = request.JoiningDate;
        if (request.TransportRouteId.HasValue) admission.TransportRouteId = request.TransportRouteId;
        admission.UpdatedAt = DateTime.UtcNow;

        // 7) Update Registration if linked
        if (registration != null)
        {
            registration.Status = "converted";
            registration.ConvertedToStudentId = student.Id;
            registration.UpdatedAt = DateTime.UtcNow;
        }

        // 8) Update Enquiry if linked (source was enquiry)
        if (admission.EnquiryId.HasValue)
        {
            var enquiry = await _dbContext.Set<EnquiryEntity>()
                .FirstOrDefaultAsync(e => e.Id == admission.EnquiryId.Value, cancellationToken);
            if (enquiry != null && enquiry.Status != "converted")
            {
                enquiry.Status = "converted";
                enquiry.UpdatedAt = DateTime.UtcNow;
            }
        }

        // 9) Audit log
        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "Admission",
            Action = "enroll",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                EnrollmentId = enrollmentId,
                StudentId = student.Id,
                ClassId = admission.ClassId,
                SectionId = admission.SectionId
            }),
            CreatedAt = DateTime.UtcNow
        });

        var response = admission.Adapt<AdmissionResponse>();
        return Result.Success(await FillComputedAsync(response, student.Id, enrollmentId, cancellationToken));
    }

    private async Task<AdmissionResponse> FillComputedAsync(AdmissionResponse r, long studentId, string enrollmentId, CancellationToken ct)
    {
        var ayName = await _dbContext.Set<AcademicYearEntity>().AsNoTracking()
            .Where(a => a.Id == r.AcademicYearId).Select(a => a.Name).FirstOrDefaultAsync(ct);
        var className = await _dbContext.Set<ClassEntity>().AsNoTracking()
            .Where(c => c.Id == r.ClassId).Select(c => c.Name).FirstOrDefaultAsync(ct);
        var sectionName = await _dbContext.Set<SectionEntity>().AsNoTracking()
            .Where(s => s.Id == r.SectionId).Select(s => s.Name).FirstOrDefaultAsync(ct);

        var studentName = await _dbContext.Set<StudentEntity>().AsNoTracking()
            .Where(s => s.Id == studentId)
            .Select(s => (s.FirstName + " " + (s.LastName ?? "")).Trim())
            .FirstOrDefaultAsync(ct);

        return r with
        {
            AcademicYearName = ayName,
            ClassName = className,
            SectionName = sectionName,
            StudentName = studentName,
            EnrollmentId = enrollmentId,
            StudentId = studentId
        };
    }
}
