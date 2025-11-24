
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using AutoPartsShop.Infrastructure;
using AutoPartsShop.Infrastructure.Data;
using Microsoft.AspNetCore.OData;
using System.Text.Json.Serialization;
using AutoPartsShop.Identity.Models;
using Microsoft.OData.ModelBuilder;
using AutoPartsShop.Domain.Dtos;
using AutoPartsShop.App.Services;

Environment.CurrentDirectory = AppContext.BaseDirectory;

var webRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");

if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// 添加基础业务服务
builder.Services.AddApplicationServices();

ODataConventionModelBuilder modelBuilder = new();
modelBuilder.EntitySet<ApplicationUserDto>("User");
modelBuilder.EntitySet<RoleDto>("Role");

builder.Services.AddControllers()
    .AddOData(options => options.EnableQueryFeatures().AddRouteComponents("odata", modelBuilder.GetEdmModel()))
    .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AutoPartsShop API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new()
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new()
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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthorization();

builder.Services.AddDbContext<AppDbContext>(options =>
{
   // options.UseSqlite(builder.Configuration.GetConnectionString("AppDbContext"));
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbContextSQLServer"));
});

builder.Services.AddRazorPages();

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddRateLimiter(options =>
{
    // Window Rate Limiter
    options.AddFixedWindowLimiter("Fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueLimit = 0;
        opt.AutoReplenishment = true;
    });

    // Sliding Window Rate Limiter
    options.AddSlidingWindowLimiter("Sliding", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 4;
        opt.QueueLimit = 2;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.SegmentsPerWindow = 2;

    });

    // Token Bucket Rate Limiter
    options.AddTokenBucketLimiter("Token", opt =>
    {
        opt.TokenLimit = 4;
        opt.QueueLimit = 2;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        opt.TokensPerPeriod = 4;
        opt.AutoReplenishment = true;

    });

    //Concurrency Limiter
    options.AddConcurrencyLimiter("Concurrency", opt =>
    {
        opt.PermitLimit = 10;
        opt.QueueLimit = 2;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;

    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);

            await context.HttpContext.Response.WriteAsync(
                $"Too many requests. Please try again after {retryAfter.TotalMinutes} minute(s). " +
                $"Read more about our rate limits at https://www.radendpoint.com/faq/.", cancellationToken: token);
        }
        else
        {
            await context.HttpContext.Response.WriteAsync(
                "Too many requests. Please try again later. " +
                "Read more about our rate limits at https://www.radendpoint.com/faq/.", cancellationToken: token);
        }
    };
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
});

builder.Services.AddScoped<ImageService>();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30 // Keep logs for 30 days
    )
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MudBlazor Shop API V1");
    });
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
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.UseAuthorization();

app.MapGroup("/identity").MapIdentityApi<ApplicationUser>().WithTags("Identity");

app.MapControllers();

app.MapFallbackToFile("index.html");

app.UseRateLimiter();
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