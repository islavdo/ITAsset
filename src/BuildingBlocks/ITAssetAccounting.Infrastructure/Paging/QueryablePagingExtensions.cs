using ITAssetAccounting.Shared.Paging;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccounting.Infrastructure.Paging;

public static class QueryablePagingExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = total
        };
    }
}
