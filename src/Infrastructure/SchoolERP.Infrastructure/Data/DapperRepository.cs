using Dapper;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Common;
using System.Data;
using System.Reflection;
using System.Text;

namespace SchoolERP.Infrastructure.Data;

public class DapperRepository<T> : IDapperRepository<T> where T : class
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICurrentTenantService _tenantService;

    public DapperRepository(IDbConnectionFactory connectionFactory, ICurrentTenantService tenantService)
    {
        _connectionFactory = connectionFactory;
        _tenantService = tenantService;
    }

    // 🔥 PRIVATE: Auto-inject TenantId into WHERE clause
    private string InjectTenantFilter(string sql, object? parameters)
    {
        // Agar entity BaseEntity se inherit karti hai toh TenantId filter apply karo
        if (typeof(T).IsSubclassOf(typeof(BaseEntity)) || typeof(T) == typeof(BaseEntity))
        {
            var hasWhere = sql.Contains("WHERE", StringComparison.OrdinalIgnoreCase);
            var tenantFilter = $"TenantId = @__TenantId__";

            // Agar parameters mein already TenantId hai toh override karo
            if (parameters == null)
                parameters = new { __TenantId__ = _tenantService.GetTenantId() };
            else
            {
                // Anonymous type mein dynamic add nahi kar sakte, isliye hum dictionary bana kar merge karenge
                var paramDict = new Dictionary<string, object>();
                foreach (var prop in parameters.GetType().GetProperties())
                    paramDict[prop.Name] = prop.GetValue(parameters)!;

                paramDict["__TenantId__"] = _tenantService.GetTenantId();
                parameters = paramDict;
            }

            if (hasWhere)
                return sql + " AND " + tenantFilter;
            else
                return sql + " WHERE " + tenantFilter;
        }

        return sql;
    }

    // ==========================================================
    // 🔥 GENERIC METHODS (With Tenant Injection)
    // ==========================================================

    public async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        var tableName = typeof(T).Name + "s"; // Naive, baad mein mapping table/attribute se kar lenge
        var sql = $"SELECT * FROM {tableName} WHERE Id = @Id";
        sql = InjectTenantFilter(sql, new { Id = id });

        return await conn.QueryFirstOrDefaultAsync<T>(sql, new { Id = id, __TenantId__ = _tenantService.GetTenantId() });
    }

    public async Task<T?> FirstOrDefaultAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        sql = InjectTenantFilter(sql, parameters);
        return await conn.QueryFirstOrDefaultAsync<T>(sql, parameters);
    }

    public async Task<IEnumerable<T>> GetListAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        sql = InjectTenantFilter(sql, parameters);
        return await conn.QueryAsync<T>(sql, parameters);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        var tableName = typeof(T).Name + "s";
        var sql = $"SELECT * FROM {tableName}";
        sql = InjectTenantFilter(sql, null);
        return await conn.QueryAsync<T>(sql, new { __TenantId__ = _tenantService.GetTenantId() });
    }

    public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? orderBy = null,
        string? whereClause = null,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        var tableName = typeof(T).Name + "s";
        var offset = (pageNumber - 1) * pageSize;

        var sql = new StringBuilder($"SELECT * FROM {tableName}");

        // Apply Tenant Filter
        sql.Append($" WHERE TenantId = @__TenantId__");

        if (!string.IsNullOrEmpty(whereClause))
            sql.Append($" AND {whereClause}");

        // Order By
        sql.Append($" ORDER BY {(string.IsNullOrEmpty(orderBy) ? "Id" : orderBy)}");

        // Pagination
        sql.Append($" OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY");

        // Count Query (for total records)
        var countSql = new StringBuilder($"SELECT COUNT(1) FROM {tableName}");
        countSql.Append($" WHERE TenantId = @__TenantId__");
        if (!string.IsNullOrEmpty(whereClause))
            countSql.Append($" AND {whereClause}");

        var paramDict = new Dictionary<string, object> { { "__TenantId__", _tenantService.GetTenantId() } };
        if (parameters != null)
        {
            foreach (var prop in parameters.GetType().GetProperties())
                paramDict[prop.Name] = prop.GetValue(parameters)!;
        }

        var items = await conn.QueryAsync<T>(sql.ToString(), paramDict);
        var total = await conn.ExecuteScalarAsync<int>(countSql.ToString(), paramDict);

        return (items, total);
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        // Execute ke liye Tenant injection nahi karte (Update/Delete mein manual WHERE lagna chahiye)
        return await conn.ExecuteAsync(sql, parameters);
    }
}