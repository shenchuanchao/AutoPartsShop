using System.Security.Claims;
using AutoPartsShop.Infrastructure;
using AutoPartsShop.Test1;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 配置基础地址
var baseAddress = builder.HostEnvironment.IsDevelopment()
    ? "https://localhost:7293"  // API 项目的开发地址
    : builder.HostEnvironment.BaseAddress;

builder.Services.AddAuthorizationCore();
// 添加基础设施层服务（包括数据库上下文和业务服务）
//builder.Services.AddInfrastructure(builder.Configuration);


builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();

// 配置 HTTP 客户端
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(baseAddress)
});

// 注册 Ant Design 服务 - 这是关键！
builder.Services.AddAntDesign();



await builder.Build().RunAsync();



public class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // 示例: 创建一个未认证的用户
        var anonymous = new ClaimsIdentity();
        var user = new ClaimsPrincipal(anonymous);

        return Task.FromResult(new AuthenticationState(user));

        // 实际应用中，你通常会在这里检查本地存储的Token、
        // 验证其有效性，并构建包含用户声明的 ClaimsPrincipal。
    }
}