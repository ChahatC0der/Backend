using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;
using RegistrationDocumentEntity = SchoolERP.Domain.Student.Registration.Entities.RegistrationDocument;

namespace SchoolERP.Application.Features.Student.Queries.RegistrationDocument.ExportRegistrationDocuments;

public class ExportRegistrationDocumentsQueryHandler : IRequestHandler<ExportRegistrationDocumentsQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly IExcelExportService _excelExportService;

    public ExportRegistrationDocumentsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentBranchService branchService,
        IExcelExportService excelExportService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
        _excelExportService = excelExportService;
    }

    public async Task<Result<byte[]>> Handle(ExportRegistrationDocumentsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var regExists = await _dbContext.Set<RegistrationEntity>()
            .AnyAsync(r => r.Id == query.RegistrationId && r.BranchId == branchId && !r.IsDeleted, cancellationToken);

        if (!regExists)
            return Error.NotFound("Registration", query.RegistrationId.ToString());

        var items = await _dbContext.Set<RegistrationDocumentEntity>()
            .AsNoTracking()
            .Where(d => d.RegistrationId == query.RegistrationId && !d.IsDeleted)
            .OrderBy(d => d.DocumentType)
            .ProjectToType<RegistrationDocumentLightResponse>()
            .ToListAsync(cancellationToken);

        var excelExport = _excelExportService.Export(items, "RegistrationDocuments");

        return Result.Success(excelExport);
    }
}
