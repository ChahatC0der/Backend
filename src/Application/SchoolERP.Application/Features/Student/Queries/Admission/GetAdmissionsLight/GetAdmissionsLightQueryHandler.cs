using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using AdmissionEntity = SchoolERP.Domain.Student.Admission.Entities.Admission;

namespace SchoolERP.Application.Features.Student.Queries.Admission.GetAdmissionsLight;

public class GetAdmissionsLightQueryHandler : IRequestHandler<GetAdmissionsLightQuery, Result<List<AdmissionLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetAdmissionsLightQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<List<AdmissionLightResponse>>> Handle(GetAdmissionsLightQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var items = await _dbContext.Set<AdmissionEntity>()
            .AsNoTracking()
            .Where(a => a.BranchId == branchId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        if (!items.Any())
            return Result.Success(new List<AdmissionLightResponse>());

        var regIds = items.Where(x => x.RegistrationId.HasValue).Select(x => x.RegistrationId!.Value).Distinct().ToList();
        var studentIds = items.Where(x => x.StudentId.HasValue).Select(x => x.StudentId!.Value).Distinct().ToList();

        var regNumbers = await _dbContext.Set<RegistrationEntity>().AsNoTracking()
            .Where(r => regIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id, r => r.RegistrationNumber, cancellationToken);
        var students = await _dbContext.Set<StudentEntity>().AsNoTracking()
            .Where(s => studentIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => new { s.EnrollmentId, FullName = (s.FirstName + " " + (s.LastName ?? "")).Trim() }, cancellationToken);

        var data = items.Select(a => new AdmissionLightResponse(
            a.Id,
            a.StudentId.HasValue && students.TryGetValue(a.StudentId.Value, out var st) ? st.FullName : null,
            a.StudentId.HasValue && students.TryGetValue(a.StudentId.Value, out var st2) ? st2.EnrollmentId : null,
            a.RegistrationId.HasValue && regNumbers.TryGetValue(a.RegistrationId.Value, out var rn) ? rn : null,
            a.ClassId,
            a.SectionId,
            a.Status,
            a.AdmissionDate)).ToList();

        return Result.Success(data);
    }
}
