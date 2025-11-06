using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AutoPartsShop;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 配置基础地址
var baseAddress = builder.HostEnvironment.IsDevelopment()
    ? "https://localhost:7293"  // API 项目的开发地址
    : builder.HostEnvironment.BaseAddress;

// 添加Ant Design服务
builder.Services.AddAntDesign();

// 配置 Ant Design 主题（可选）
//builder.Services.Configure<AntDesign.Theme>(options =>
//{
//    options.PrimaryColor = "#1890ff";
//    options.BorderRadiusBase = 4;
//    options.LinkColor = "#1890ff";
//});

// 添加本地存储服务
builder.Services.AddBlazoredLocalStorage(config =>
{
    config.JsonSerializerOptions.WriteIndented = true;
});

// 配置 HTTP 客户端
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(baseAddress)
});

// 注册自定义 HTTP 客户端（带认证）
//builder.Services.AddHttpClient("AuthHttpClient", client =>
//{
//    client.BaseAddress = new Uri(baseAddress);
//    client.DefaultRequestHeaders.Add("User-Agent", "AutoPartsShop-BlazorWASM");
//});

// 身份认证和授权服务
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
//builder.Services.AddScoped<IAuthService, AuthService>();

// 注册应用服务

// 配置日志
if (builder.HostEnvironment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
    builder.Logging.AddFilter("Microsoft.AspNetCore.Components", LogLevel.Warning);
}


// 构建并运行应用
var host = builder.Build();

// 初始化服务（如果需要）
//await InitializeServices(host);

await host.RunAsync();

// 初始化服务的辅助方法
async Task InitializeServices(WebAssemblyHost host)
{
    try
    {
        // 这里可以添加应用启动时需要初始化的服务
        // 例如：检查认证状态、加载用户配置等

        //var authService = host.Services.GetRequiredService<IAuthService>();
        //var localStorage = host.Services.GetRequiredService<ILocalStorageService>();

        //// 检查是否有保存的token并设置认证状态
        //var token = await authService.GetTokenAsync();
        //if (!string.IsNullOrEmpty(token))
        //{
        //    Console.WriteLine("检测到已保存的认证令牌");
        //}
    }
    catch (Exception ex)
    {
        Console.WriteLine($"服务初始化失败: {ex.Message}");
    }
}
