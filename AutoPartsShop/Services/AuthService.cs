using AutoPartsShop.Domain.Common;
using AutoPartsShop.Domain.Dtos;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AutoPartsShop.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILocalStorageService _localStorage;
        public AuthService(HttpClient httpClient,
                          AuthenticationStateProvider authenticationStateProvider,
                          ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
        }


        /// <summary>
        /// 客户登录
        /// </summary>
        /// <param name="loginRequest"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest loginRequest)
        {
            try
            {
                // 输入验证
                if (loginRequest == null)
                    throw new ArgumentNullException(nameof(loginRequest), "登录请求不能为空");

                if (string.IsNullOrWhiteSpace(loginRequest.Email))
                    throw new ArgumentException("用户名不能为空", nameof(loginRequest.Email));

                if (string.IsNullOrWhiteSpace(loginRequest.Password))
                    throw new ArgumentException("密码不能为空", nameof(loginRequest.Password));

                var response = await _httpClient.PostAsJsonAsync("/auth/login", loginRequest);
                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    await _localStorage.SetItemAsync("authToken", loginResponse.Token);
                    ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(loginRequest.Email);
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
                    return ApiResponse<LoginResponse>.Success(loginResponse);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return ApiResponse<LoginResponse>.Failure(errorMessage, 401);
                }
                else
                {
                    // 处理非成功状态码
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {errorMessage}");
                    throw new Exception($"登录失败: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                // 处理网络错误
                Console.WriteLine($"Request Error: {ex.Message}");
                throw new Exception("无法连接到服务器，请检查网络连接");
            }
            catch (Exception ex)
            {
                // 处理其他异常
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }

        }
        /// <summary>
        /// 用户登出
        /// </summary>
        /// <returns></returns>
        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

    }
}
