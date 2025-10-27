using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPartsShop.Core.Interfaces
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string username, string password);
        Task LogoutAsync();
        Task<bool> RegisterAsync(string username, string password, string email);
        Task<string> GetTokenAsync();

    }
}
