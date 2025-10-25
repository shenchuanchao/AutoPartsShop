using Microsoft.EntityFrameworkCore;
using AutoPartsShop.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using AutoPartsShop.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// 添加基础设施层服务（包括数据库上下文和业务服务）
builder.Services.AddInfrastructure(builder.Configuration);

// 在API项目中注册Identity服务
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // 密码策略
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    // 用户配置
    options.User.RequireUniqueEmail = true;

    // 登录配置
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;

    // 锁定配置
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 添加控制器和API探索
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();

// 配置Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AutoPartsShop API",
        Version = "v1",
        Description = "汽修配件商城系统 API"
    });

    // 添加JWT认证到Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// 配置CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins("https://localhost:7000", "http://localhost:5000") // Blazor应用的地址
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JWT身份认证配置
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// 授权策略
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("CustomerOnly", policy =>
        policy.RequireRole("Customer"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI(c =>
    //{
    //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoPartsShop API v1");
    //    c.RoutePrefix = "swagger"; // 在/swagger访问Swagger UI
    //});
    // 开发环境下自动应用数据库迁移
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("正在检查数据库...");

            // 确保数据库已创建
            if (!dbContext.Database.CanConnect())
            {
                logger.LogInformation("数据库不存在，正在创建...");
                await dbContext.Database.EnsureCreatedAsync();
                logger.LogInformation("数据库创建完成");
            }
            else
            {
                logger.LogInformation("数据库已存在，检查表结构...");

                // 检查是否有表
                var tables = dbContext.Model.GetEntityTypes();
                logger.LogInformation("发现的实体类型: {Count}", tables.Count());

                foreach (var entityType in tables)
                {
                    logger.LogInformation("实体: {Entity}", entityType.Name);
                }

                // 应用迁移
                logger.LogInformation("正在应用迁移...");
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("迁移应用完成");
            }

            // 初始化角色和用户
            logger.LogInformation("正在初始化角色和用户...");
            await InitializeRolesAndUsers(scope.ServiceProvider);
            logger.LogInformation("角色和用户初始化完成");
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "数据库初始化过程中发生错误");
        }
    }
}

app.UseHttpsRedirection();

// 启用CORS（必须在UseAuthentication和UseAuthorization之前）
app.UseCors("AllowBlazorApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// 初始化角色和用户的辅助方法
async Task InitializeRolesAndUsers(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // 创建角色
    string[] roleNames = { "Admin", "Customer", "Vendor" };
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // 创建默认管理员用户
    var adminUser = await userManager.FindByEmailAsync("admin@autopartsshop.com");
    if (adminUser == null)
    {
        var user = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@autopartsshop.com",
            FullName = "系统管理员",
            EmailConfirmed = true
        };

        var createPowerUser = await userManager.CreateAsync(user, "Admin123!");
        if (createPowerUser.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}