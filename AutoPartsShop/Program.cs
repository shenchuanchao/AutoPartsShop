using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AutoPartsShop;
using AntDesign;
using Microsoft.AspNetCore.Components.Authorization;
using AutoPartsShop.Infrastructure.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ����HTTP�ͻ���
builder.Services.AddHttpClient("AutoPartsShop.API",
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

// ���Ant Design����
builder.Services.AddAntDesign();

// �����֤����
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// ע��Ӧ�÷���
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<ICartService, CartService>();
builder.Services.AddTransient<IOrderService, OrderService>();



builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
