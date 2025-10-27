
using AutoPartsShop.Test;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ���û�����ַ
var baseAddress = builder.HostEnvironment.IsDevelopment()
    ? "https://localhost:7293"  // API ��Ŀ�Ŀ�����ַ
    : builder.HostEnvironment.BaseAddress;

// ���Ant Design����
builder.Services.AddAntDesign();

// ���� Ant Design ���⣨��ѡ��
//builder.Services.Configure<AntDesign.Theme>(options =>
//{
//    options.PrimaryColor = "#1890ff";
//    options.BorderRadiusBase = 4;
//    options.LinkColor = "#1890ff";
//});

// ��ӱ��ش洢����
builder.Services.AddBlazoredLocalStorage(config =>
{
    config.JsonSerializerOptions.WriteIndented = true;
});

// ���� HTTP �ͻ���
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(baseAddress)
});

// ע���Զ��� HTTP �ͻ��ˣ�����֤��
builder.Services.AddHttpClient("AuthHttpClient", client =>
{
    client.BaseAddress = new Uri(baseAddress);
    client.DefaultRequestHeaders.Add("User-Agent", "AutoPartsShop-BlazorWASM");
});

// �����֤����Ȩ����
builder.Services.AddAuthorizationCore();

// ע��Ӧ�÷���

// ������־
if (builder.HostEnvironment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
    builder.Logging.AddFilter("Microsoft.AspNetCore.Components", LogLevel.Warning);
}


// ����������Ӧ��
var host = builder.Build();


await host.RunAsync();


