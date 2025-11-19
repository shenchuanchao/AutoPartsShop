using AutoPartsShop.Mud;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazorServices(builder.HostEnvironment.BaseAddress);
builder.Services.AddBrowserStorageService();

await builder.Build().RunAsync();
