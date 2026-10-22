using Microsoft.EntityFrameworkCore;
using MonitoringApi.Contracts.Requests;

namespace MonitoringApi.Infrastructure.Persistence;

public static class QueryableExtensions
{
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
