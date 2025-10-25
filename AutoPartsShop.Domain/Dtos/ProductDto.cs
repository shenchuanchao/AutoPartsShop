using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPartsShop.Domain.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string YearRange { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string YearRange { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class UpdateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string YearRange { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }


}
