using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsShop.Domain.Common
{
    public class PagedList<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;

        public PagedList(List<T> items, int count, int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
            TotalCount = count;
            Items = items;
        }

        public static async Task<PagedList<T>> CreateAsync(
            IQueryable<T> source, int page, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<T>(items, count, page, pageSize);
        }
    }
}
