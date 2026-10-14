using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;

namespace SchoolERP.Application.Features.Student.Queries.Registration.GetRegistrationById;

public class GetRegistrationByIdQueryHandler : IRequestHandler<GetRegistrationByIdQuery, Result<RegistrationResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetRegistrationByIdQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<RegistrationResponse>> Handle(GetRegistrationByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var reg = await _dbContext.Set<RegistrationEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == query.Id && r.BranchId == branchId.Value && !r.IsDeleted, cancellationToken);

        if (reg == null)
            return Error.NotFound("Registration", query.Id.ToString());

        var response = reg.Adapt<RegistrationResponse>();

        var className = await _dbContext.Set<ClassEntity>().AsNoTracking()
            .Where(c => c.Id == reg.ClassId).Select(c => c.Name).FirstOrDefaultAsync(cancellationToken);

        string? sectionName = null;
        if (reg.SectionId.HasValue)
            sectionName = await _dbContext.Set<SectionEntity>().AsNoTracking()
                .Where(s => s.Id == reg.SectionId.Value).Select(s => s.Name).FirstOrDefaultAsync(cancellationToken);

        string? interviewerName = null;
        if (reg.InterviewerId.HasValue)
            interviewerName = await _dbContext.Set<UserEntity>().AsNoTracking()
                .Where(u => u.Id == reg.InterviewerId.Value).Select(u => u.Name).FirstOrDefaultAsync(cancellationToken);

        string? approvedByName = null;
        if (reg.ApprovedBy.HasValue)
            approvedByName = await _dbContext.Set<UserEntity>().AsNoTracking()
                .Where(u => u.Id == reg.ApprovedBy.Value).Select(u => u.Name).FirstOrDefaultAsync(cancellationToken);

        return Result.Success(response with
        {
            ClassName = className,
            SectionName = sectionName,
            InterviewerName = interviewerName,
            ApprovedByName = approvedByName
        });
    }
}
