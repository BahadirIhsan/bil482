using IndustryApp.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace IndustryApp.Infrastructure.Persistence.Repositories;

internal static class PagingExtensions
{
    /// <summary>Sorguyu sayfalar; sayfa ve boyut 1'in altındaysa 1'e çekilir. Üst sınır request validator'larındadır.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, IPagedQuery paging, CancellationToken ct)
    {
        var page = Math.Max(1, paging.Page);
        var pageSize = Math.Max(1, paging.PageSize);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<T>(items, page, pageSize, total);
    }
}
