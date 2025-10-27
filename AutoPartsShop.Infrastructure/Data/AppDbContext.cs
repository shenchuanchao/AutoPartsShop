using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AutoPartsShop.Domain.Models;

namespace AutoPartsShop.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 产品配置
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.SKU).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
                entity.Property(p => p.OriginalPrice).HasColumnType("decimal(18,2)");
                entity.Property(p => p.Brand).HasMaxLength(100);
                entity.Property(p => p.VehicleModel).HasMaxLength(100);
                entity.Property(p => p.YearRange).HasMaxLength(50);
                entity.Property(p => p.ImageUrl).HasMaxLength(500);

                entity.HasIndex(p => p.SKU).IsUnique();

                // 关系配置
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 分类配置
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Description).HasMaxLength(500);
                entity.Property(c => c.ImageUrl).HasMaxLength(200);

                // 自引用关系
                entity.HasOne(c => c.ParentCategory)
                      .WithMany(c => c.SubCategories)
                      .HasForeignKey(c => c.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 订单配置
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(o => o.ShippingAddress).IsRequired().HasMaxLength(500);
                entity.Property(o => o.RecipientPhone).HasMaxLength(20);
                entity.Property(o => o.RecipientName).HasMaxLength(100);
                entity.Property(o => o.Note).HasMaxLength(1000);

                entity.HasIndex(o => o.OrderNumber).IsUnique();

                // 关系配置
                entity.HasMany(o => o.OrderItems)
                      .WithOne(oi => oi.Order)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 订单项配置
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id);
                entity.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(oi => oi.ProductName).HasMaxLength(200);
                entity.Property(oi => oi.ProductImage).HasMaxLength(500);

                // 关系配置
                entity.HasOne(oi => oi.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 购物车配置
            modelBuilder.Entity<ShoppingCart>(entity =>
            {
                entity.HasKey(sc => sc.Id);

                // 关系配置
                entity.HasMany(sc => sc.CartItems)
                      .WithOne(ci => ci.ShoppingCart)
                      .HasForeignKey(ci => ci.CartId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 购物车项配置
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(ci => ci.Id);

                // 关系配置
                entity.HasOne(ci => ci.Product)
                      .WithMany(p => p.CartItems)
                      .HasForeignKey(ci => ci.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 种子数据
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // 使用固定的日期时间，而不是动态的 DateTime.UtcNow
            var fixedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            // 添加默认分类
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "发动机部件", Description = "发动机相关配件", SortOrder = 1, CreatedAt = fixedDate },
                new Category { Id = 2, Name = "制动系统", Description = "刹车系统配件", SortOrder = 2, CreatedAt = fixedDate },
                new Category { Id = 3, Name = "悬挂系统", Description = "悬挂和减震配件", SortOrder = 3, CreatedAt = fixedDate },
                new Category { Id = 4, Name = "电气系统", Description = "电子和电气配件", SortOrder = 4, CreatedAt = fixedDate }
            );

            // 添加示例产品
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "丰田卡罗拉机油滤清器",
                    Description = "适用于丰田卡罗拉 2014-2019 款",
                    SKU = "TOY-COR-OF-001",
                    Price = 45.99m,
                    StockQuantity = 100,
                    CategoryId = 1,
                    Brand = "丰田",
                    VehicleModel = "卡罗拉",
                    YearRange = "2014-2019",
                    ImageUrl = "/images/products/oil-filter.jpg",
                    CreatedAt = fixedDate 
                },
                new Product
                {
                    Id = 2,
                    Name = "本田思域刹车片",
                    Description = "前轮刹车片，适用于本田思域 2015-2021 款",
                    SKU = "HON-CIV-BP-001",
                    Price = 89.99m,
                    OriginalPrice = 99.99m,
                    StockQuantity = 50,
                    CategoryId = 2,
                    Brand = "本田",
                    VehicleModel = "思域",
                    YearRange = "2015-2021",
                    ImageUrl = "/images/products/brake-pads.jpg",
                    CreatedAt = fixedDate 
                }
            );
        }
    }

   }