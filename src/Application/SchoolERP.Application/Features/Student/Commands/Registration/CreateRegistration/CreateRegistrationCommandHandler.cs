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

namespace SchoolERP.Application.Features.Student.Commands.Registration.CreateRegistration;

public class CreateRegistrationCommandHandler : IRequestHandler<CreateRegistrationCommand, Result<RegistrationResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreateRegistrationCommandHandler(
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

    public async Task<Result<RegistrationResponse>> Handle(CreateRegistrationCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        // Validate academic year, class, section
        var ayErr = await _dbContext.EnsureEntityExistsAsync<AcademicYearEntity>(request.AcademicYearId, cancellationToken);
        if (ayErr != null) return Error.NotFound("AcademicYear", request.AcademicYearId.ToString());

        var classErr = await _dbContext.EnsureEntityExistsAsync<ClassEntity>(request.ClassId, cancellationToken);
        if (classErr != null) return Error.NotFound("Class", request.ClassId.ToString());

        if (request.SectionId.HasValue)
        {
            var secErr = await _dbContext.EnsureEntityExistsAsync<SectionEntity>(request.SectionId.Value, cancellationToken);
            if (secErr != null) return Error.NotFound("Section", request.SectionId.Value.ToString());
        }

        var registration = request.Adapt<RegistrationEntity>();
        //registration.TenantId = tenantId;
        registration.RegistrationNumber = await GenerateRegistrationNumberAsync(branchId.Value, cancellationToken);
        registration.Status = "draft";

        _dbContext.Set<RegistrationEntity>().Add(registration);

        var userId = _currentUserService.GetUserId() ?? 0;
        //_dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = userId,
        //    Resource = "Registration",
        //    Action = "create",
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new
        //    {
        //        registration.RegistrationNumber,
        //        registration.StudentFirstName,
        //        registration.ParentPrimaryMobile
        //    }),
        //    CreatedAt = DateTime.UtcNow
        //});

        var response = registration.Adapt<RegistrationResponse>();
        return Result.Success(await FillComputedAsync(response, cancellationToken));
    }

    private async Task<string> GenerateRegistrationNumberAsync(Guid branchId, CancellationToken ct)
    {
        var branch = await _dbContext.Set<SchoolERP.Domain.Tenants.Entities.Branch>()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(b => b.Id == branchId)
            .Select(b => b.Code)
            .FirstOrDefaultAsync(ct) ?? "BR";

        var year = DateTime.UtcNow.Year;

        var count = await _dbContext.Set<RegistrationEntity>()
            .IgnoreQueryFilters()
            .Where(r => r.BranchId == branchId && r.CreatedAt.Year == year)
            .CountAsync(ct);

        return $"{branch}-REG-{year}-{(count + 1):D4}";
    }

    private async Task<RegistrationResponse> FillComputedAsync(RegistrationResponse r, CancellationToken ct)
    {
        string? className = null;
        string? sectionName = null;
        string? interviewerName = null;
        string? approvedByName = null;

        className = await _dbContext.Set<ClassEntity>().AsNoTracking()
            .Where(c => c.Id == r.ClassId).Select(c => c.Name).FirstOrDefaultAsync(ct);

        if (r.SectionId.HasValue)
            sectionName = await _dbContext.Set<SectionEntity>().AsNoTracking()
                .Where(s => s.Id == r.SectionId.Value).Select(s => s.Name).FirstOrDefaultAsync(ct);

        if (r.InterviewerId.HasValue)
            interviewerName = await _dbContext.Set<SchoolERP.Domain.Rbac.Entities.User>().AsNoTracking()
                .Where(u => u.Id == r.InterviewerId.Value).Select(u => u.Name).FirstOrDefaultAsync(ct);

        if (r.ApprovedBy.HasValue)
            approvedByName = await _dbContext.Set<SchoolERP.Domain.Rbac.Entities.User>().AsNoTracking()
                .Where(u => u.Id == r.ApprovedBy.Value).Select(u => u.Name).FirstOrDefaultAsync(ct);

        return r with { ClassName = className, SectionName = sectionName, InterviewerName = interviewerName, ApprovedByName = approvedByName };
    }
}
