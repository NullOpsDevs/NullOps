using Microsoft.EntityFrameworkCore;
using NullOps.DAL.Models;
using NullOps.DataContract;
using NullOps.DataContract.Request;
using NullOps.DataContract.Response;

namespace NullOps.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedResponse<TDto>> ToPagedResponseAsync<T, TDto>(this IQueryable<T> query, Paging paging)
        where T : IEntity
        where TDto : class, IMappable<T, TDto>
    {
        var page = paging.Page <= 0 ? 1 : paging.Page;
        var pageSize = paging.PageSize <= 0 ? 10 : paging.PageSize;

        var totalCount = await query.CountAsync();
        var skip = (page - 1) * pageSize;

        var data = await query
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync();

        return new PagedResponse<TDto>
        {
            Data = data.Select(TDto.MapTo),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}