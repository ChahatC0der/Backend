using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using AlumniEntity = SchoolERP.Domain.Student.Alumni.Entities.Alumni;

namespace SchoolERP.Application.Features.Student.Queries.Alumni.GetAlumniById;

public class GetAlumniByIdQueryHandler : IRequestHandler<GetAlumniByIdQuery, Result<AlumniResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetAlumniByIdQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<AlumniResponse>> Handle(GetAlumniByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var alumni = await _dbContext.Set<AlumniEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == query.Id && a.BranchId == branchId && !a.IsDeleted, cancellationToken);

        if (alumni == null)
            return Error.NotFound("Alumni", query.Id.ToString());

        var className = await _dbContext.Set<ClassEntity>().AsNoTracking()
            .Where(c => c.Id == alumni.LastClassId).Select(c => c.Name).FirstOrDefaultAsync(cancellationToken);

        var ayName = await _dbContext.Set<AcademicYearEntity>().AsNoTracking()
            .Where(a => a.Id == alumni.PassingAcademicYearId).Select(a => a.Name).FirstOrDefaultAsync(cancellationToken);

        var response = alumni.Adapt<AlumniResponse>();
        return Result.Success(response with { LastClassName = className, PassingAcademicYearName = ayName });
    }
}
