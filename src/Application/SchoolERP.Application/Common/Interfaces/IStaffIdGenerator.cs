namespace SchoolERP.Application.Common.Interfaces;

public interface IStaffIdGenerator
{
    /// <summary>
    /// Generates a unique Staff ID based on tenant/branch settings and format.
    /// </summary>
    Task<string> GenerateAsync(Guid branchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a Staff ID using a specific format (used for preview or overrides).
    /// </summary>
    Task<string> GenerateAsync(Guid branchId, string format, CancellationToken cancellationToken = default);
}