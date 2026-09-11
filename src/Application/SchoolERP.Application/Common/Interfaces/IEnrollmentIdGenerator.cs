namespace SchoolERP.Application.Common.Interfaces;

public interface IEnrollmentIdGenerator
{
    Task<string> GenerateAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<string> GenerateAsync(Guid branchId, string format, CancellationToken cancellationToken = default);
}
