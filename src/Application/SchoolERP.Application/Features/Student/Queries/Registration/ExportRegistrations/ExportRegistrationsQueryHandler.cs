using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;

namespace SchoolERP.Application.Features.Student.Queries.Registration.ExportRegistrations;

public class ExportRegistrationsQueryHandler : IRequestHandler<ExportRegistrationsQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly IExcelExportService _excelExportService;

    public ExportRegistrationsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentBranchService branchService,
        IExcelExportService excelExportService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
        _excelExportService = excelExportService;
    }

    public async Task<Result<byte[]>> Handle(ExportRegistrationsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var items = await _dbContext.Set<RegistrationEntity>()
            .AsNoTracking()
            .Where(r => r.BranchId == branchId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ProjectToType<RegistrationLightResponse>()
            .ToListAsync(cancellationToken);

        var excelExport = _excelExportService.Export(items, "Registrations");

        return Result.Success(excelExport);
    }
}
