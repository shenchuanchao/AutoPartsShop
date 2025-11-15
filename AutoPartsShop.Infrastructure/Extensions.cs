using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPartsShop.Core.Interfaces;
using AutoPartsShop.Infrastructure.Services;
using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;

namespace AutoPartsShop.Infrastructure
{
    public static class Extensions
    {
        public static void AddBrowserStorageService(this IServiceCollection services)
        {
            services.AddBlazoredLocalStorage();
            services.AddScoped<IStorageService, BrowserStorageService>();
        }
    }
}
