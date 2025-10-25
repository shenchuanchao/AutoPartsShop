using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AutoPartsShop.Domain.Models
{
    public class ShoppingCart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        // 计算属性
        [NotMapped]
        public int TotalItems => CartItems?.Sum(ci => ci.Quantity) ?? 0;

        [NotMapped]
        public decimal TotalPrice => CartItems?.Sum(ci => ci.TotalPrice) ?? 0;
    }
}
