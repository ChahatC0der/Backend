using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Common.Extensions;

public static class UniquenessCheckExtensions
{
    /// <summary>
    /// Sirf EK check ke liye. Agar duplicate mila, Error return karega, warna null.
    /// </summary>
    public static async Task<Error?> EnsureUniqueAsync<TEntity>(
        this IApplicationDbContext dbContext,
        Expression<Func<TEntity, bool>> predicate,
        string conflictMessage,
        CancellationToken cancellationToken) where TEntity : class
    {
        var exists = await dbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);
        return exists ? Error.Conflict(conflictMessage) : null;
    }

    /// <summary>
    /// Multiple checks ek saath. Jitne bhi duplicates milein, sabki error messages 
    /// ek Error mein combine karke return karega (" | " se separated), warna null.
    /// </summary>
    public static async Task<Error?> EnsureAllUniqueAsync<TEntity>(
        this IApplicationDbContext dbContext,
        IEnumerable<(Expression<Func<TEntity, bool>> Predicate, string Message)> checks,
        CancellationToken cancellationToken) where TEntity : class
    {
        var failedMessages = new List<string>();

        foreach (var (predicate, message) in checks)
        {
            var exists = await dbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);
            if (exists)
                failedMessages.Add(message);
        }

        return failedMessages.Count > 0
            ? Error.Conflict(string.Join(" | ", failedMessages))
            : null;
    }
}