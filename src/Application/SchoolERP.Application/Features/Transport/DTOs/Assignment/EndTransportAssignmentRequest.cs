namespace SchoolERP.Application.Features.Transport.DTOs.Assignment;

public sealed class EndTransportAssignmentRequest
{
    public DateOnly? EndDate { get; set; }   // null = today
}
