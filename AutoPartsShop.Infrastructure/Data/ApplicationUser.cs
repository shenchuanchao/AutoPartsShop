using AutoPartsShop.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace AutoPartsShop.Infrastructure.Data
{
    // 扩展ApplicationUser类（如果需要自定义用户属性）
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        // 导航属性
        public virtual ShoppingCart? ShoppingCart { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    }
}
