using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AutoPartsShop.Domain.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CartId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        [ForeignKey("CartId")]
        public virtual ShoppingCart ShoppingCart { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        // 计算属性
        [NotMapped]
        public decimal UnitPrice => Product?.Price ?? 0;

        [NotMapped]
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
