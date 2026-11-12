
using SchoolERP.Domain.Student.Entities;
using SchoolERP.Domain.Fees.Entities;

public sealed record InvoiceGenerationContext(

    // Student whose invoice is being generated.
    Student Student,

    // Enrollment applicable to the requested billing period.
    StudentEnrollment Enrollment,

    // Fee structure resolved for the student's enrollment.
    FeeStructure FeeStructure,

    // Fee heads referenced by the active fee structure lines.
    IReadOnlyDictionary<long, FeeHead> FeeHeads,

    // Approved concession allocations assigned to the student.
    IReadOnlyList<ConcessionAllocation> ConcessionAllocations,

    // Concession definitions applicable on the evaluation date.
    IReadOnlyDictionary<long, Concession> Concessions,

    // OneTime exactly-once support:
    // Source line ids already billed on any NON-CANCELLED invoice
    // of this student + AY. Empty set = kuch bhi bill nahi hua abhi.
    // InvoiceBuilder isme se OneTime line skip karne ka decision leta hai.
    HashSet<long> AlreadyBilledSourceLineIds
);