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
// ��ӻ�����ʩ����񣨰������ݿ������ĺ�ҵ�����
builder.Services.AddInfrastructure(builder.Configuration);

// ��API��Ŀ��ע��Identity����
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // �������
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    // �û�����
    options.User.RequireUniqueEmail = true;

    // ��¼����
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;

    // ��������
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ��ӿ�������API̽��
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();

// ����Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AutoPartsShop API",
        Version = "v1",
        Description = "��������̳�ϵͳ API"
    });

    // ���JWT��֤��Swagger
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

// ����CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins("https://localhost:7000", "http://localhost:5000") // BlazorӦ�õĵ�ַ
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JWT�����֤����
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

// ��Ȩ����
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
    //    c.RoutePrefix = "swagger"; // ��/swagger����Swagger UI
    //});
    // �����������Զ�Ӧ�����ݿ�Ǩ��
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("���ڼ�����ݿ�...");

            // ȷ�����ݿ��Ѵ���
            if (!dbContext.Database.CanConnect())
            {
                logger.LogInformation("���ݿⲻ���ڣ����ڴ���...");
                await dbContext.Database.EnsureCreatedAsync();
                logger.LogInformation("���ݿⴴ�����");
            }
            else
            {
                logger.LogInformation("���ݿ��Ѵ��ڣ�����ṹ...");

                // ����Ƿ��б�
                var tables = dbContext.Model.GetEntityTypes();
                logger.LogInformation("���ֵ�ʵ������: {Count}", tables.Count());

                foreach (var entityType in tables)
                {
                    logger.LogInformation("ʵ��: {Entity}", entityType.Name);
                }

                // Ӧ��Ǩ��
                logger.LogInformation("����Ӧ��Ǩ��...");
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Ǩ��Ӧ�����");
            }

            // ��ʼ����ɫ���û�
            logger.LogInformation("���ڳ�ʼ����ɫ���û�...");
            await InitializeRolesAndUsers(scope.ServiceProvider);
            logger.LogInformation("��ɫ���û���ʼ�����");
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "���ݿ��ʼ�������з�������");
        }
    }
}

app.UseHttpsRedirection();

// ����CORS��������UseAuthentication��UseAuthorization֮ǰ��
app.UseCors("AllowBlazorApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// ��ʼ����ɫ���û��ĸ�������
async Task InitializeRolesAndUsers(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // ������ɫ
    string[] roleNames = { "Admin", "Customer", "Vendor" };
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // ����Ĭ�Ϲ���Ա�û�
    var adminUser = await userManager.FindByEmailAsync("admin@autopartsshop.com");
    if (adminUser == null)
    {
        var user = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@autopartsshop.com",
            FullName = "ϵͳ����Ա",
            EmailConfirmed = true
        };

        var createPowerUser = await userManager.CreateAsync(user, "Admin123!");
        if (createPowerUser.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}