using AutoPartsShop.Domain.Common;
using AutoPartsShop.Domain.Dtos;
using AutoPartsShop.Domain.Models;
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
            return await _httpClient.GetFromJsonAsync<PagedList<ProductDto>>("api/products");
        }




    }
}
