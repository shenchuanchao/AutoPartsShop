using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AutoPartsShop.Domain.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string ProductImage { get; set; } = string.Empty;

        // 导航属性
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        // 计算属性
        [NotMapped]
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
