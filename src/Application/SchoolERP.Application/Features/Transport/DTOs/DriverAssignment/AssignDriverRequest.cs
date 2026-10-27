namespace SchoolERP.Application.Features.Transport.DTOs.DriverAssignment;

public sealed class AssignDriverRequest
{
    public long DriverStaffId { get; set; }
    public DateOnly ValidFrom { get; set; }
}
