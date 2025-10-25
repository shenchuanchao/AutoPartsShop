using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AutoPartsShop.Domain.Models
{
    /// <summary>
    /// 商品实体
    /// </summary>
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string SKU { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }

        [Required]
        public int StockQuantity { get; set; } = 0;

        [Required]
        public int CategoryId { get; set; }

        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [MaxLength(100)]
        public string VehicleModel { get; set; } = string.Empty;

        [MaxLength(50)]
        public string YearRange { get; set; } = string.Empty;

        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // 导航属性
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
