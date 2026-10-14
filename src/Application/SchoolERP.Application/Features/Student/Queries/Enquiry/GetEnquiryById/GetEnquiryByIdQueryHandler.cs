using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using EnquiryEntity = SchoolERP.Domain.Student.Enquiry.Entities.Enquiry;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;

namespace SchoolERP.Application.Features.Student.Queries.Enquiry.GetEnquiryById;

public class GetEnquiryByIdQueryHandler : IRequestHandler<GetEnquiryByIdQuery, Result<EnquiryResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetEnquiryByIdQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<EnquiryResponse>> Handle(GetEnquiryByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var enquiry = await _dbContext.Set<EnquiryEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == query.Id && e.BranchId == branchId.Value && !e.IsDeleted, cancellationToken);

        if (enquiry == null)
            return Error.NotFound("Enquiry", query.Id.ToString());

        string? className = null;
        if (enquiry.ClassId.HasValue)
            className = await _dbContext.Set<ClassEntity>().AsNoTracking()
                .Where(c => c.Id == enquiry.ClassId.Value).Select(c => c.Name).FirstOrDefaultAsync(cancellationToken);

        string? assignedToName = null;
        if (enquiry.AssignedTo.HasValue)
            assignedToName = await _dbContext.Set<UserEntity>().AsNoTracking()
                .Where(u => u.Id == enquiry.AssignedTo.Value).Select(u => u.Name).FirstOrDefaultAsync(cancellationToken);

        return Result.Success(new EnquiryResponse(
            enquiry.Id, enquiry.BranchId, enquiry.EnquiryDate, enquiry.Source,
            enquiry.StudentFirstName, enquiry.StudentLastName,
            enquiry.ParentName, enquiry.Mobile, enquiry.Email,
            enquiry.ClassId, className,
            enquiry.Message,
            enquiry.AssignedTo, assignedToName,
            enquiry.Status, enquiry.ConvertedToRegistrationId,
            enquiry.CreatedAt, enquiry.UpdatedAt));
    }
}
