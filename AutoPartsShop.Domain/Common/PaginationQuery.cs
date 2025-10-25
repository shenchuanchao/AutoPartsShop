using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPartsShop.Domain.Models;

namespace AutoPartsShop.Domain.Common
{
    public class PaginationQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
    }

    public class ProductQuery : PaginationQuery
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public string? Brand { get; set; }
        public string? VehicleModel { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStockOnly { get; set; }
    }

    public class OrderQuery : PaginationQuery
    {
        public string? UserId { get; set; }
        public OrderStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

}
