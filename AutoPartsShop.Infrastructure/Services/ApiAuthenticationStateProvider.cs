using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace AutoPartsShop.Infrastructure.Services
{
    public class ApiAuthenticationStateProvider: AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // 示例: 创建一个未认证的用户
            var anonymous = new ClaimsIdentity();
            var user = new ClaimsPrincipal(anonymous);

            return Task.FromResult(new AuthenticationState(user));

            // 实际应用中，你通常会在这里检查本地存储的Token、
            // 验证其有效性，并构建包含用户声明的 ClaimsPrincipal。
        }
    }
}
