
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using SchoolERP.Domain.Fees.Entities;
using SchoolERP.Domain.Student.Entities;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Contains all data required to construct an invoice draft.
///
/// This is an in-memory context only.
/// It does not perform database queries, business calculations,
/// or invoice persistence.
/// </summary>
public sealed record InvoiceGenerationContext(
    // Student whose invoice is being generated.
    StudentEntity Student,

    // Enrollment applicable to the requested billing period.
    StudentEnrollment Enrollment,

    // Fee structure resolved for the student's enrollment.
    FeeStructure FeeStructure,

    // Fee heads referenced by the active fee structure lines.
    IReadOnlyDictionary<long, FeeHead> FeeHeads,

    // Approved concession allocations assigned to the student.
    IReadOnlyList<ConcessionAllocation> ConcessionAllocations,

    // Concession definitions applicable on the evaluation date.
    IReadOnlyDictionary<long, Concession> Concessions
);
