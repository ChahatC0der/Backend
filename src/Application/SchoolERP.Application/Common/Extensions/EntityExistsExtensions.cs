using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Common.Extensions;

public static class EntityExistsExtensions
{
    /// <summary>
    /// Checks if an entity exists by Id (returns Error if not found).
    /// 🔥 Supports Guid, long, int — ALL ID TYPES.
    /// </summary>
    public static async Task<Error?> EnsureEntityExistsAsync<TEntity>(
        this IApplicationDbContext dbContext,
        object id,
        CancellationToken cancellationToken) where TEntity : class
    {
        var exists = await dbContext.Set<TEntity>()
            .AnyAsync(e => EF.Property<object>(e, "Id").Equals(id), cancellationToken);

        if (exists)
            return null;
        
        var entityName = typeof(TEntity).Name;
        return Error.NotFound(entityName, id.ToString()!);
    }

    /// <summary>
    /// Fetches entity by Id, returns Result<T> (Success/Failure).
    /// 🔥 Supports Guid, long, int — ALL ID TYPES.
    /// </summary>
    public static async Task<Result<TEntity>> GetEntityByIdAsync<TEntity>(
        this IApplicationDbContext dbContext,
        object id,
        CancellationToken cancellationToken) where TEntity : class
    {
        var entity = await dbContext.Set<TEntity>()
            .FirstOrDefaultAsync(e => EF.Property<object>(e, "Id").Equals(id), cancellationToken);

        if (entity == null)
        {
            var entityName = typeof(TEntity).Name;
            return Error.NotFound(entityName, id.ToString()!);
        }

        return Result.Success(entity);
    }

    /// <summary>
    /// Fetches entity using custom predicate, returns Result<T> (Success/Failure).
    /// ❌ No automatic IsDeleted filter — developer must include it in predicate.
    /// </summary>
    public static async Task<Result<TEntity>> GetEntityAsync<TEntity>(
        this IApplicationDbContext dbContext,
        Expression<Func<TEntity, bool>> predicate,
        string entityName,
        string identifier,
        CancellationToken cancellationToken) where TEntity : class
    {
        var entity = await dbContext.Set<TEntity>()
            .FirstOrDefaultAsync(predicate, cancellationToken);

        if (entity == null)
            return Error.NotFound(entityName, identifier);

        return Result.Success(entity);
    }
}