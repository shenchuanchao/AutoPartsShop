
using AutoPartsShop.Test;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;


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
builder.Services.AddHttpClient("AuthHttpClient", client =>
{
    client.BaseAddress = new Uri(baseAddress);
    client.DefaultRequestHeaders.Add("User-Agent", "AutoPartsShop-BlazorWASM");
});

// 身份认证和授权服务
builder.Services.AddAuthorizationCore();

// 注册应用服务

// 配置日志
if (builder.HostEnvironment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
    builder.Logging.AddFilter("Microsoft.AspNetCore.Components", LogLevel.Warning);
}


// 构建并运行应用
var host = builder.Build();


await host.RunAsync();


