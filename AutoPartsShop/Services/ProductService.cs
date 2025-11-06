using AutoPartsShop.Domain.Common;
using AutoPartsShop.Domain.Dtos;
using System.Net.Http.Json;

namespace AutoPartsShop.Services
{
    /// <summary>
    /// 商品服务
    /// </summary>
    public class ProductService
    {
        private readonly HttpClient _httpClient;
        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// 获取所有商品
        /// </summary>
        /// <returns></returns>
        public async Task<PagedList<ProductDto>> GetProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/product");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PagedList<ProductDto>>();
                }
                else
                {
                    // 处理非成功状态码
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {errorMessage}");
                    throw new Exception($"获取产品失败: {response.StatusCode}");
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
        /// 获取热门商品
        /// </summary>
        /// <param name="totalNums"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<ProductDto>> GetHotProductsAsync(int totalNums)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/product/hot/{totalNums}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ProductDto>>();
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {errorMessage}");
                    throw new Exception($"获取产品失败: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request Error: {ex.Message}");
                throw new Exception("无法连接到服务器，请检查网络连接");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }



    }
}
