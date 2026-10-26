using SchoolERP.Application.Features.Fees.DTOs.FeeStructure;
using FeeStructureEntity = SchoolERP.Domain.Fees.Entities.FeeStructure;
using FeeStructureLineEntity = SchoolERP.Domain.Fees.Entities.FeeStructureLine;

namespace SchoolERP.Application.Features.Fees.DTOs.FeeStructure;

/// <summary>Single mapping point entity → response. Used by all FeeStructure handlers.</summary>
public static class FeeStructureResponseFactory
{
    public static FeeStructureResponse ToResponse(
        FeeStructureEntity e,
        IReadOnlyDictionary<long, (string Name, string Code)>? feeHeadLookup = null)
    {
        return new FeeStructureResponse
        {
            Id = e.Id,
            AcademicYearId = e.AcademicYearId,
            ClassGroupId = e.ClassGroupId,
            ClassId = e.ClassId,
            SectionId = e.SectionId,
            Name = e.Name,
            ApplicableFrom = e.ApplicableFrom,
            ApplicableTo = e.ApplicableTo,
            ProrationMethod = e.ProrationMethod.ToString(),
            PaymentAllocationPriority = e.PaymentAllocationPriority.ToString(),
            Status = e.Status.ToString(),
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            Lines = e.Lines.Select(l => ToLineResponse(l, feeHeadLookup)).ToList(),
        };
    }

    public static FeeStructureLineResponse ToLineResponse(
        FeeStructureLineEntity l,
        IReadOnlyDictionary<long, (string Name, string Code)>? feeHeadLookup = null)
    {
        var (name, code) = (string.Empty, string.Empty);
        if (feeHeadLookup is not null && feeHeadLookup.TryGetValue(l.FeeHeadId, out var h))
        {
            name = h.Name; code = h.Code;
        }
        return new FeeStructureLineResponse
        {
            Id = l.Id,
            FeeHeadId = l.FeeHeadId,
            FeeHeadName = name,
            FeeHeadCode = code,
            Amount = l.Amount,
            Frequency = l.Frequency.ToString(),
            DueDay = l.DueDay,
            LateFeeApplicable = l.LateFeeApplicable,
            LateFeeType = l.LateFeeType?.ToString(),
            LateFeeValue = l.LateFeeValue,
            LateFeeGraceDays = l.LateFeeGraceDays,
            LateFeeCapPercent = l.LateFeeCapPercent,
            LateFeeSlabConfig = l.LateFeeSlabConfig,
        };
    }
}
