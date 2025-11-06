using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace AutoPartsShop
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // 这里实现您的身份验证逻辑
            // 示例：未认证状态
            var anonymous = new ClaimsIdentity();
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonymous)));

            // 如果已认证，可以这样返回：
            // var user = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "用户名") }, "认证类型");
            // return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(user)));
        }
    }
}
