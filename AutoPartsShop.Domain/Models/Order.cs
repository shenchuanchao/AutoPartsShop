using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AutoPartsShop.Domain.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        [MaxLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;

        [MaxLength(20)]
        public string RecipientPhone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string RecipientName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Note { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // 导航属性
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // 计算属性（不在数据库中存储）
        [NotMapped]
        public int TotalItems => OrderItems?.Sum(oi => oi.Quantity) ?? 0;
    }

    public enum OrderStatus
    {
        Pending = 0,    // 待支付
        Paid = 1,       // 已支付
        Shipped = 2,    // 已发货
        Completed = 3,  // 已完成
        Cancelled = 4,  // 已取消
        Refunded = 5    // 已退款
    }
}
