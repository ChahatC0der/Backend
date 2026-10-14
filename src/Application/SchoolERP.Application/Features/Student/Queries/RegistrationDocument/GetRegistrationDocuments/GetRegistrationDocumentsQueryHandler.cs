using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;
using RegistrationDocumentEntity = SchoolERP.Domain.Student.Registration.Entities.RegistrationDocument;

namespace SchoolERP.Application.Features.Student.Queries.RegistrationDocument.GetRegistrationDocuments;

public class GetRegistrationDocumentsQueryHandler : IRequestHandler<GetRegistrationDocumentsQuery, Result<PagedResponse<RegistrationDocumentResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetRegistrationDocumentsQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<PagedResponse<RegistrationDocumentResponse>>> Handle(GetRegistrationDocumentsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var request = query.Request;

        var q = from d in _dbContext.Set<RegistrationDocumentEntity>()
                join r in _dbContext.Set<RegistrationEntity>() on d.RegistrationId equals r.Id
                where !d.IsDeleted && !r.IsDeleted && r.BranchId == branchId.Value
                select d;

        if (query.RegistrationId.HasValue)
            q = q.Where(d => d.RegistrationId == query.RegistrationId.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var s = request.SearchTerm.ToLower();
            q = q.Where(d => d.DocumentType.ToLower().Contains(s) || d.Status.ToLower().Contains(s));
        }

        q = request.SortBy?.ToLower() switch
        {
            "documenttype" => request.SortOrder?.ToLower() == "desc" ? q.OrderByDescending(d => d.DocumentType) : q.OrderBy(d => d.DocumentType),
            "status" => request.SortOrder?.ToLower() == "desc" ? q.OrderByDescending(d => d.Status) : q.OrderBy(d => d.Status),
            _ => q.OrderBy(d => d.DocumentType)
        };

        var totalCount = await q.CountAsync(cancellationToken);
        var items = await q.Skip((request.Page - 1) * request.Size).Take(request.Size).ToListAsync(cancellationToken);

        var data = items.Select(d => new RegistrationDocumentResponse(
            d.Id, d.RegistrationId, d.DocumentType, d.FileUrl, d.Description,
            d.Status, d.RejectionReason, d.VerifiedBy, d.VerifiedAt,
            d.CreatedAt, d.UpdatedAt)).ToList();

        return Result.Success(new PagedResponse<RegistrationDocumentResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}
