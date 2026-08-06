using System.Linq.Expressions;

namespace SchoolERP.Application.Common.Interfaces;

public interface IDapperRepository<T> where T : class
{
    // 🔥 Single Record
    Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default);

    // 🔥 Multiple Records
    Task<IEnumerable<T>> GetListAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    // 🔥 Paged (For Reports/Grids)
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? orderBy = null,
        string? whereClause = null,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    // 🔥 Execute (Insert/Update/Delete - for Dapper raw operations, though EF Core writes are preferred)
    Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default);
}