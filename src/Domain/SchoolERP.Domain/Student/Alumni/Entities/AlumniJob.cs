using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Alumni.Entities;

public class AlumniJob : BranchEntity
{
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? ApplicationLink { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public long? CreatedBy { get; set; }
}
