using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.VehicleDocument;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Transport.Enums;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;
using VehicleDocumentEntity = SchoolERP.Domain.Transport.Entities.VehicleDocument;

namespace SchoolERP.Application.Features.Transport.Commands.VehicleDocument.UploadVehicleDocument;

/// <summary>
/// Upload/replace a vehicle document. Since (VehicleId, DocumentType) is unique,
/// uploading for an existing type replaces FileUrl + ExpiryDate instead of inserting a duplicate.
/// Also syncs the vehicle summary date (InsuranceValidUntil / PucValidUntil / FitnessValidUntil)
/// from the document ExpiryDate — summary is a denormalized copy for fast list/dashboard queries.
/// </summary>
public sealed class UploadVehicleDocumentCommandHandler : IRequestHandler<UploadVehicleDocumentCommand, Result<VehicleDocumentResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentUserService _user;

    public UploadVehicleDocumentCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant,
        ICurrentBranchService branch, ICurrentUserService user)
    { _db = db; _tenant = tenant; _branch = branch; _user = user; }

    public async Task<Result<VehicleDocumentResponse>> Handle(UploadVehicleDocumentCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        if (!TryParseDocumentType(request.Request.DocumentType, out var docType))
            return Error.Validation($"Invalid DocumentType: {request.Request.DocumentType}.");

        // Load vehicle first — scoped through branch
        var vehicle = await _db.Set<VehicleEntity>().FirstOrDefaultAsync(v =>
            v.Id == request.VehicleId && v.TenantId == tenantId
            && v.BranchId == branchId && !v.IsDeleted, ct);
        if (vehicle is null) return Error.NotFound("Vehicle", request.VehicleId.ToString());

        // Check existing document for (VehicleId, DocumentType)
        var existing = await _db.Set<VehicleDocumentEntity>().FirstOrDefaultAsync(d =>
            d.VehicleId == request.VehicleId && !d.IsDeleted, ct
        );
        if (existing is not null && existing.DocumentType != docType)
            existing = null;

        var now = DateTime.UtcNow;
        if (existing is not null)
        {
            existing.FileUrl = request.Request.FileUrl.Trim();
            existing.ExpiryDate = request.Request.ExpiryDate;
            existing.UploadedBy = _user.GetUserId();
        }
        else
        {
            existing = new VehicleDocumentEntity
            {
                VehicleId = request.VehicleId,
                DocumentType = docType,
                FileUrl = request.Request.FileUrl.Trim(),
                ExpiryDate = request.Request.ExpiryDate,
                UploadedBy = _user.GetUserId(),
                CreatedAt = now,
            };
            _db.Set<VehicleDocumentEntity>().Add(existing);
        }

        // Sync summary column on Vehicle
        if (docType == VehicleDocumentType.Insurance) vehicle.InsuranceValidUntil = request.Request.ExpiryDate;
        else if (docType == VehicleDocumentType.Puc) vehicle.PucValidUntil = request.Request.ExpiryDate;
        else if (docType == VehicleDocumentType.Fitness) vehicle.FitnessValidUntil = request.Request.ExpiryDate;
        vehicle.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);
        // TODO (audit): await _auditWriter.LogAsync("VehicleDocuments", existing.Id, FinancialAuditAction.Update, null, existing, ct);

        return Result.Success(VehicleDocumentResponse.FromEntity(existing));
    }

    internal static bool TryParseDocumentType(string value, out VehicleDocumentType type)
    {
        foreach (var kv in EnumMaps_JsonAdapter.VehicleDocumentTypeMap)
            if (string.Equals(kv.Value, value, StringComparison.OrdinalIgnoreCase)) { type = kv.Key; return true; }
        type = default; return false;
    }
}
