using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPartsShop.Core.Interfaces;

namespace AutoPartsShop.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        public Task<bool> LoginAsync(string username, string password)
        {
            throw new NotImplementedException();
        }
        public Task LogoutAsync()
        {
            throw new NotImplementedException();
        }
        public Task<bool> RegisterAsync(string username, string password, string email)
        {
            throw new NotImplementedException();
        }
        public Task<string> GetTokenAsync()
        {
            throw new NotImplementedException();
        }
    }
}
