using Microsoft.EntityFrameworkCore;

namespace ACT.Infrastructure.Extensions;

public static class QueryablePagingExtensions
{
    public const int MaxPageSize = 100;

    /// <summary>
    /// Counts and pages a query with page/pageSize clamped to sane bounds, so a caller can't
    /// force a full-table scan/serialize with e.g. ?pageSize=999999999.
    /// </summary>
    public static async Task<(List<T> Items, int TotalCount)> ToPagedResultAsync<T>(
        this IQueryable<T> query, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, totalCount);
    }
}
