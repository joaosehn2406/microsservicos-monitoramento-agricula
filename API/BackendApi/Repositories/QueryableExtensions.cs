using BackendApi.DTOs.Requests;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Repositories;

public static class QueryableExtensions
{
    /// <summary>Counts the filtered query and loads one page of it; the query must already be ordered.</summary>
    public static async Task<(IReadOnlyList<T> Items, int TotalItems)> ToPageAsync<T>(
        this IQueryable<T> query,
        PageQuery page,
        CancellationToken cancellationToken)
    {
        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page.Page - 1) * page.PageSize)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }
}
