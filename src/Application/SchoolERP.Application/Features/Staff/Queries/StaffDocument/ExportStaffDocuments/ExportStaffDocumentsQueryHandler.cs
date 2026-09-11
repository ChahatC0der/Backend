using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;
using StaffDocumentEntity = SchoolERP.Domain.Staff.Entities.StaffDocument;

namespace SchoolERP.Application.Features.Staff.Queries.StaffDocument.ExportStaffDocuments;

public class ExportStaffDocumentsQueryHandler : IRequestHandler<ExportStaffDocumentsQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public ExportStaffDocumentsQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<byte[]>> Handle(ExportStaffDocumentsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var staffExists = await _dbContext.Set<StaffEntity>()
            .AnyAsync(s => s.Id == query.StaffId && s.BranchId == branchId && !s.IsDeleted, cancellationToken);
        if (!staffExists)
            return Error.NotFound("Staff", query.StaffId.ToString());

        var items = await _dbContext.Set<StaffDocumentEntity>()
            .AsNoTracking()
            .Where(d => d.StaffId == query.StaffId && !d.IsDeleted)
            .OrderBy(d => d.DocumentType)
            .ProjectToType<StaffDocumentLightResponse>()
            .ToListAsync(cancellationToken);

        var csvBytes = items.ToCsv();

        return Result.Success(csvBytes);
    }
}
