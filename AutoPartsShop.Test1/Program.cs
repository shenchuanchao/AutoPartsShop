using System.Security.Claims;
using AutoPartsShop.Infrastructure;
using AutoPartsShop.Test1;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ���û�����ַ
var baseAddress = builder.HostEnvironment.IsDevelopment()
    ? "https://localhost:7293"  // API ��Ŀ�Ŀ�����ַ
    : builder.HostEnvironment.BaseAddress;

builder.Services.AddAuthorizationCore();
// ��ӻ�����ʩ����񣨰������ݿ������ĺ�ҵ�����
//builder.Services.AddInfrastructure(builder.Configuration);


builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();

// ���� HTTP �ͻ���
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(baseAddress)
});

// ע�� Ant Design ���� - ���ǹؼ���
builder.Services.AddAntDesign();



await builder.Build().RunAsync();



public class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // ʾ��: ����һ��δ��֤���û�
        var anonymous = new ClaimsIdentity();
        var user = new ClaimsPrincipal(anonymous);

        return Task.FromResult(new AuthenticationState(user));

        // ʵ��Ӧ���У���ͨ�����������鱾�ش洢��Token��
        // ��֤����Ч�ԣ������������û������� ClaimsPrincipal��
    }
}