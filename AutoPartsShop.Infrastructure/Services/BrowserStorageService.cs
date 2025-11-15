using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPartsShop.Core.Interfaces;
using Blazored.LocalStorage;

namespace AutoPartsShop.Infrastructure.Services
{
    public class BrowserStorageService(ILocalStorageService localStorage) : IStorageService
    {
        public Task<T?> GetAsync<T>(string key)
        {
            return localStorage.GetItemAsync<T>(key).AsTask();
        }

        public Task SetAsync<T>(string key, T value)
        {
            return localStorage.SetItemAsync(key, value).AsTask();
        }

        public Task RemoveAsync(string key)
        {
            return localStorage.RemoveItemAsync(key).AsTask();
        }

        public Task ClearAsync()
        {
            return localStorage.ClearAsync().AsTask();
        }
    }
}
