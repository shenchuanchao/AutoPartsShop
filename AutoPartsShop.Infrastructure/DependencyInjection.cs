using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using AutoPartsShop.Core.Interfaces;
using AutoPartsShop.Infrastructure.Services;
using AutoPartsShop.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using AutoPartsShop.Identity.Models;

namespace AutoPartsShop.Infrastructure
{
    public static class DependencyInjection
    {
        /// <summary>
        /// 注册基础设施服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 数据库上下文
            services.AddDbContext<AppDbContext>(options =>
            // options.UseSqlite(builder.Configuration.GetConnectionString("AppDbContext"));
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                        //sqlOptions.MigrationsAssembly("AutoPartsShop.Infrastructure");
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));
            // 注册 Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // 密码策略配置
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            return services;
        }

        /// <summary>
        /// 注册业务服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services) 
        {
            // 注册业务服务
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IRoleService, RoleService>();


            return services;
        }


    }
}