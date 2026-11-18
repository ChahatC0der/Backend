
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.Assignment;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;
using TransportAssignmentEntity = SchoolERP.Domain.Transport.Entities.TransportAssignment;
using TransportFeeEntity = SchoolERP.Domain.Transport.Entities.TransportFee;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Transport.Queries.Assignment.GetStudentTransportFeeQuote;

/// <summary>
/// Resolves effective transport fee:
/// 1. Stop fee takes priority over route fee.
/// 2. Same pickup/drop route and stop are charged only once.
/// 3. Different pickup/drop routes use the maximum fee.
/// 4. Returns null when no assignment or fee is configured.
/// </summary>
public sealed class GetStudentTransportFeeQuoteQueryHandler
    : IRequestHandler<
        GetStudentTransportFeeQuoteQuery,
        Result<TransportAssignmentFeeQuoteResponse?>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public GetStudentTransportFeeQuoteQueryHandler(
        IApplicationDbContext db,
        ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<Result<TransportAssignmentFeeQuoteResponse?>> Handle(
        GetStudentTransportFeeQuoteQuery request,
        CancellationToken ct)
    {
        // 1. Resolve tenant context.
        var tenantId = _tenant.GetTenantId();

        if (tenantId == Guid.Empty)
            return Error.Unauthorized(
                "Tenant context is not resolved.");

        if (request.StudentId <= 0)
            return Error.Validation(
                "Student ID must be greater than zero.");

        if (request.AcademicYearId <= 0)
            return Error.Validation(
                "Academic Year ID must be greater than zero.");

        // 2. Get the student's branch.
        var student = await _db.Set<StudentEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.Id == request.StudentId &&
                s.TenantId == tenantId &&
                !s.IsDeleted,
                ct);

        if (student is null)
            return Error.NotFound("Student",request.StudentId.ToString());

        var branchId = student.BranchId;

        // 3. Find the current transport assignment.
        var assignment = await _db.Set<TransportAssignmentEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a =>
                a.TenantId == tenantId &&
                a.StudentId == request.StudentId &&
                a.AcademicYearId == request.AcademicYearId &&
                a.IsCurrent &&
                !a.IsDeleted,
                ct);

        if (assignment is null)
        {
            return Result.Success<TransportAssignmentFeeQuoteResponse?>(
                null);
        }

        // 4. Resolve pickup fee.
        var pickup = await ResolveFeeAsync(
            branchId,
            assignment.PickupStopId,
            assignment.PickupRouteId,
            request.AcademicYearId,
            ct);

        // 5. Resolve drop fee only when drop location differs.
        var dropRouteId =
            assignment.DropRouteId ?? assignment.PickupRouteId;

        var dropStopId =
            assignment.DropStopId ?? assignment.PickupStopId;

        var drop = new FeeResult(0, null, null, null);

        var hasDifferentDropLocation =
            dropRouteId != assignment.PickupRouteId ||
            dropStopId != assignment.PickupStopId;

        if (hasDifferentDropLocation)
        {
            drop = await ResolveFeeAsync(
                branchId,
                dropStopId,
                dropRouteId,
                request.AcademicYearId,
                ct);
        }

        // 6. No fee configured.
        if (pickup.Amount <= 0 && drop.Amount <= 0)
        {
            return Result.Success<TransportAssignmentFeeQuoteResponse?>(
                null);
        }

        // 7. Apply maximum fee policy.
        var usedMax = drop.Amount > pickup.Amount;

        var selectedFee = usedMax ? drop : pickup;

        var finalAmount = Math.Max(
            pickup.Amount,
            drop.Amount);

        // 8. Build response.
        return Result.Success<TransportAssignmentFeeQuoteResponse?>(
            new TransportAssignmentFeeQuoteResponse
            {
                StudentId = request.StudentId,
                AcademicYearId = request.AcademicYearId,
                AssignmentId = assignment.Id,

                Amount = finalAmount,
                Frequency = selectedFee.Frequency ?? "monthly",

                SourceType = usedMax ? "drop" : "pickup",

                RouteId = selectedFee.RouteId,
                StopId = selectedFee.StopId,

                PickupAmount = pickup.Amount,
                DropAmount = drop.Amount,

                UsedMaxPolicy = usedMax,

                TransportFeeHeadCode = "TRANSPORT"
            });
    }

    private sealed record FeeResult(
        decimal Amount,
        string? Frequency,
        long? RouteId,
        long? StopId);

    private async Task<FeeResult> ResolveFeeAsync(
        Guid branchId,
        long stopId,
        long routeId,
        long academicYearId,
        CancellationToken ct)
    {
        // 1. Stop-level fee has priority.
        var stopFee = await _db.Set<TransportFeeEntity>()
            .AsNoTracking()
            .Where(f =>
                f.BranchId == branchId &&
                f.AcademicYearId == academicYearId &&
                f.StopId == stopId &&
                !f.IsDeleted)
            .Select(f => new
            {
                f.FeeAmount,
                Frequency = f.Frequency.ToString(),
                f.RouteId,
                f.StopId
            })
            .FirstOrDefaultAsync(ct);

        if (stopFee is not null)
        {
            return new FeeResult(
                stopFee.FeeAmount,
                stopFee.Frequency,
                stopFee.RouteId,
                stopFee.StopId);
        }

        // 2. Fallback to route-level fee.
        var routeFee = await _db.Set<TransportFeeEntity>()
            .AsNoTracking()
            .Where(f =>
                f.BranchId == branchId &&
                f.AcademicYearId == academicYearId &&
                f.RouteId == routeId &&
                !f.IsDeleted)
            .Select(f => new
            {
                f.FeeAmount,
                Frequency = f.Frequency.ToString(),
                f.RouteId,
                f.StopId
            })
            .FirstOrDefaultAsync(ct);

        if (routeFee is not null)
        {
            return new FeeResult(
                routeFee.FeeAmount,
                routeFee.Frequency,
                routeFee.RouteId,
                routeFee.StopId);
        }

        return new FeeResult(
            0,
            null,
            null,
            null);
    }
}
