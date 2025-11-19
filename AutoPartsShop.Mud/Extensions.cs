using AutoPartsShop.Core.Interfaces;
using AutoPartsShop.Infrastructure.Services;
using AutoPartsShop.Mud.Authorization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;

namespace AutoPartsShop.Mud;

public static class Extensions
{

    public static void AddBlazorServices(this IServiceCollection services, string baseAddress)
    {

        services.AddScoped(sp
            => new HttpClient { BaseAddress = new Uri(baseAddress) });

        services.AddAuthorizationCore();
        services.AddScoped<AuthenticationStateProvider, IdentityAuthenticationStateProvider>();
        services.AddScoped<ThemeService>();
        services.AddMudServices();
    }


    public static void AddBrowserStorageService(this IServiceCollection services)
    {
        services.AddBlazoredLocalStorage();
        services.AddScoped<IStorageService, BrowserStorageService>();
    }
}
