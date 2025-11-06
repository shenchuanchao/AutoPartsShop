using AutoPartsShop.Domain.Common;
using AutoPartsShop.Domain.Dtos;
using System.Net.Http.Json;

namespace AutoPartsShop.Services
{
    /// <summary>
    /// 分类服务
    /// </summary>
    public class CategoryService
    {
        private readonly HttpClient _httpClient;
        public CategoryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        /// <summary>
        /// 获取所有分类
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/category");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
                }
                else
                {
                    // 处理非成功状态码
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {errorMessage}");
                    throw new Exception($"获取分类失败: {response.StatusCode}");
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



    }
}
