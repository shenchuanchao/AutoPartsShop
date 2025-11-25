using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AutoPartsShop.Infrastructure.Data;

// 扩展ApplicationUser类（如果需要自定义用户属性）
public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }

    public string? CompanyName { get; set; }
    public string? Photo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }


}
