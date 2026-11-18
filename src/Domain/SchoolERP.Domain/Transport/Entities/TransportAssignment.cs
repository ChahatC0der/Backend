using SchoolERP.Domain.Common;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;

namespace SchoolERP.Domain.Transport.Entities;

public sealed class TransportAssignment : BaseTenantEntity
{
    public long StudentId { get; set; }
    public long AcademicYearId { get; set; }

    public long PickupVehicleId { get; set; }
    public long PickupRouteId { get; set; }
    public long PickupStopId { get; set; }

    public long? DropVehicleId { get; set; }
    public long? DropRouteId { get; set; }
    public long? DropStopId { get; set; }

    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsCurrent { get; set; }

    // Navigation Properties
    public StudentEntity Student { get; set; } = null!;

    public Vehicle PickupVehicle { get; set; } = null!;
    public Route PickupRoute { get; set; } = null!;
    public Stop PickupStop { get; set; } = null!;

    public Vehicle? DropVehicle { get; set; }
    public Route? DropRoute { get; set; }
    public Stop? DropStop { get; set; }
}