using AutoPartsShop.Domain.Common;
using AutoPartsShop.Domain.Dtos;

namespace AutoPartsShop.Core.Interfaces
{
    public interface IProductService
    {
        Task<PagedList<ProductDto>> GetProductsAsync(ProductQuery query);
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(CreateProductRequest request);
        Task<ProductDto> UpdateProductAsync(int id, UpdateProductRequest request);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> UpdateProductStockAsync(int productId, int newStockQuantity);
        Task<List<string>> GetBrandsAsync();
        Task<List<string>> GetVehicleModelsAsync();
    }
}
