using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPartsShop.Mud.Services;

public interface IStorageService
{
    public Task<T?> GetAsync<T>(string key);

    public Task SetAsync<T>(string key, T value);

    public Task RemoveAsync(string key);

    public Task ClearAsync();
}
