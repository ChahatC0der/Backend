using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;
using AdmissionEntity = SchoolERP.Domain.Student.Admission.Entities.Admission;

namespace SchoolERP.Application.Features.Student.Queries.Admission.GetAdmissions;

public class GetAdmissionsQueryHandler : IRequestHandler<GetAdmissionsQuery, Result<PagedResponse<AdmissionResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetAdmissionsQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<PagedResponse<AdmissionResponse>>> Handle(GetAdmissionsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var request = query.Request;

        var q = _dbContext.Set<AdmissionEntity>()
            .AsNoTracking()
            .Where(a => a.BranchId == branchId && !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var s = request.SearchTerm.ToLower();
            q = q.Where(a =>
                (a.RollNumber != null && a.RollNumber.ToLower().Contains(s)) ||
                (a.AdmissionNumber != null && a.AdmissionNumber.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
            q = q.Where(a => a.Status == request.Status);

        if (request.ClassId.HasValue)
            q = q.Where(a => a.ClassId == request.ClassId.Value);

        if (request.SectionId.HasValue)
            q = q.Where(a => a.SectionId == request.SectionId.Value);

        if (request.AcademicYearId.HasValue)
            q = q.Where(a => a.AcademicYearId == request.AcademicYearId.Value);

        if (request.FromDate.HasValue)
            q = q.Where(a => a.CreatedAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            q = q.Where(a => a.CreatedAt <= request.ToDate.Value);

        q = request.SortBy?.ToLower() switch
        {
            "status" => request.SortOrder?.ToLower() == "desc" ? q.OrderByDescending(a => a.Status) : q.OrderBy(a => a.Status),
            "rollnumber" => request.SortOrder?.ToLower() == "desc" ? q.OrderByDescending(a => a.RollNumber) : q.OrderBy(a => a.RollNumber),
            "admissiondate" => request.SortOrder?.ToLower() == "desc" ? q.OrderByDescending(a => a.AdmissionDate) : q.OrderBy(a => a.AdmissionDate),
            _ => q.OrderByDescending(a => a.CreatedAt)
        };

        var totalCount = await q.CountAsync(cancellationToken);

        var rawItems = await q
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        // Lookups
        var ayIds = rawItems.Select(x => x.AcademicYearId).Distinct().ToList();
        var classIds = rawItems.Select(x => x.ClassId).Distinct().ToList();
        var sectionIds = rawItems.Select(x => x.SectionId).Distinct().ToList();
        var regIds = rawItems.Where(x => x.RegistrationId.HasValue).Select(x => x.RegistrationId!.Value).Distinct().ToList();
        var studentIds = rawItems.Where(x => x.StudentId.HasValue).Select(x => x.StudentId!.Value).Distinct().ToList();
        var userIds = rawItems.Where(x => x.EnrolledBy.HasValue).Select(x => x.EnrolledBy!.Value).Distinct().ToList();

        var ayNames = await _dbContext.Set<AcademicYearEntity>().AsNoTracking()
            .Where(a => ayIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id, a => a.Name, cancellationToken);
        var classNames = await _dbContext.Set<ClassEntity>().AsNoTracking()
            .Where(c => classIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);
        var sectionNames = await _dbContext.Set<SectionEntity>().AsNoTracking()
            .Where(s => sectionIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);
        var regNumbers = await _dbContext.Set<RegistrationEntity>().AsNoTracking()
            .Where(r => regIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id, r => r.RegistrationNumber, cancellationToken);
        var students = await _dbContext.Set<StudentEntity>().AsNoTracking()
            .Where(s => studentIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => new { s.EnrollmentId, FullName = (s.FirstName + " " + (s.LastName ?? "")).Trim() }, cancellationToken);
        var userNames = await _dbContext.Set<UserEntity>().AsNoTracking()
            .Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Name, cancellationToken);

        var data = rawItems.Select(a => new AdmissionResponse(
            a.Id, a.TenantId, a.BranchId,
            a.AcademicYearId,
            ayNames.TryGetValue(a.AcademicYearId, out var ay) ? ay : null,
            a.RegistrationId,
            a.RegistrationId.HasValue && regNumbers.TryGetValue(a.RegistrationId.Value, out var rn) ? rn : null,
            a.EnquiryId,
            a.StudentId,
            a.StudentId.HasValue && students.TryGetValue(a.StudentId.Value, out var st) ? st.FullName : null,
            a.StudentId.HasValue && students.TryGetValue(a.StudentId.Value, out var st2) ? st2.EnrollmentId : null,
            a.ClassId,
            classNames.TryGetValue(a.ClassId, out var cn) ? cn : null,
            a.SectionId,
            sectionNames.TryGetValue(a.SectionId, out var sn) ? sn : null,
            a.AdmissionDate, a.AdmissionNumber, a.RollNumber, a.JoiningDate, a.PreviousSchool,
            a.FirstFeePaid, a.FirstFeeAmount, a.FirstFeePaymentDate, a.FirstFeePaymentMode,
            a.TransportRouteId, a.EnrolledAt, a.EnrolledBy,
            a.EnrolledBy.HasValue && userNames.TryGetValue(a.EnrolledBy.Value, out var un) ? un : null,
            a.Status, a.CreatedAt, a.UpdatedAt)).ToList();

        return Result.Success(new PagedResponse<AdmissionResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}
