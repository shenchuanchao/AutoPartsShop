using Microsoft.EntityFrameworkCore;
using AutoPartsShop.Core.Interfaces;
using AutoPartsShop.Domain.Common;
using AutoPartsShop.Domain.Dtos;
using AutoPartsShop.Domain.Models;
using Microsoft.Extensions.Logging;
using AutoPartsShop.Infrastructure.Data;

namespace AutoPartsShop.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(AppDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedList<ProductDto>> GetProductsAsync(ProductQuery query)
        {
            try
            {
                var productsQuery = _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive)
                    .AsQueryable();

                // 应用筛选条件
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    productsQuery = productsQuery.Where(p =>
                        p.Name.Contains(query.SearchTerm) ||
                        p.Description.Contains(query.SearchTerm) ||
                        p.Brand.Contains(query.SearchTerm) ||
                        p.VehicleModel.Contains(query.SearchTerm));
                }

                if (query.CategoryId.HasValue)
                {
                    productsQuery = productsQuery.Where(p => p.CategoryId == query.CategoryId);
                }

                if (!string.IsNullOrWhiteSpace(query.Brand))
                {
                    productsQuery = productsQuery.Where(p => p.Brand == query.Brand);
                }

                if (!string.IsNullOrWhiteSpace(query.VehicleModel))
                {
                    productsQuery = productsQuery.Where(p => p.VehicleModel == query.VehicleModel);
                }

                if (query.MinPrice.HasValue)
                {
                    productsQuery = productsQuery.Where(p => p.Price >= query.MinPrice.Value);
                }

                if (query.MaxPrice.HasValue)
                {
                    productsQuery = productsQuery.Where(p => p.Price <= query.MaxPrice.Value);
                }

                if (query.InStockOnly == true)
                {
                    productsQuery = productsQuery.Where(p => p.StockQuantity > 0);
                }

                // 应用排序
                productsQuery = query.SortBy?.ToLower() switch
                {
                    "price" => query.SortDescending
                        ? productsQuery.OrderByDescending(p => p.Price)
                        : productsQuery.OrderBy(p => p.Price),
                    "name" => query.SortDescending
                        ? productsQuery.OrderByDescending(p => p.Name)
                        : productsQuery.OrderBy(p => p.Name),
                    "created" => query.SortDescending
                        ? productsQuery.OrderByDescending(p => p.CreatedAt)
                        : productsQuery.OrderBy(p => p.CreatedAt),
                    _ => productsQuery.OrderByDescending(p => p.CreatedAt)
                };

                var totalCount = await productsQuery.CountAsync();
                var products = await productsQuery
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        SKU = p.SKU,
                        Price = p.Price,
                        OriginalPrice = p.OriginalPrice,
                        StockQuantity = p.StockQuantity,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.Name,
                        Brand = p.Brand,
                        VehicleModel = p.VehicleModel,
                        YearRange = p.YearRange,
                        ImageUrl = p.ImageUrl,
                        IsActive = p.IsActive,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();

                return new PagedList<ProductDto>(products, totalCount, query.Page, query.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取产品列表时发生错误");
                throw;
            }
        }
        /// <summary>
        /// 获取热门商品
        /// </summary>
        /// <param name="totalNums"></param>
        /// <returns></returns>
        public async Task<List<ProductDto>> GetHotProductsAsync(int totalNums) 
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.CreatedAt) // 假设按创建时间排序作为热门商品的标准
                    .Take(totalNums)
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        SKU = p.SKU,
                        Price = p.Price,
                        OriginalPrice = p.OriginalPrice,
                        StockQuantity = p.StockQuantity,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.Name,
                        Brand = p.Brand,
                        VehicleModel = p.VehicleModel,
                        YearRange = p.YearRange,
                        ImageUrl = p.ImageUrl,
                        IsActive = p.IsActive,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取热门商品时发生错误");
                throw;
            }
        }
        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

                if (product == null) return null;

                return new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    SKU = product.SKU,
                    Price = product.Price,
                    OriginalPrice = product.OriginalPrice,
                    StockQuantity = product.StockQuantity,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category.Name,
                    Brand = product.Brand,
                    VehicleModel = product.VehicleModel,
                    YearRange = product.YearRange,
                    ImageUrl = product.ImageUrl,
                    IsActive = product.IsActive,
                    CreatedAt = product.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取产品详情时发生错误，产品ID: {ProductId}", id);
                throw;
            }
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
        {
            try
            {
                // 检查SKU是否已存在
                if (await _context.Products.AnyAsync(p => p.SKU == request.SKU))
                {
                    throw new InvalidOperationException($"SKU '{request.SKU}' 已存在");
                }

                var product = new Product
                {
                    Name = request.Name,
                    Description = request.Description,
                    SKU = request.SKU,
                    Price = request.Price,
                    OriginalPrice = request.OriginalPrice,
                    StockQuantity = request.StockQuantity,
                    CategoryId = request.CategoryId,
                    Brand = request.Brand,
                    VehicleModel = request.VehicleModel,
                    YearRange = request.YearRange,
                    ImageUrl = request.ImageUrl,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // 重新加载包含分类信息
                await _context.Entry(product)
                    .Reference(p => p.Category)
                    .LoadAsync();

                return new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    SKU = product.SKU,
                    Price = product.Price,
                    OriginalPrice = product.OriginalPrice,
                    StockQuantity = product.StockQuantity,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category.Name,
                    Brand = product.Brand,
                    VehicleModel = product.VehicleModel,
                    YearRange = product.YearRange,
                    ImageUrl = product.ImageUrl,
                    IsActive = product.IsActive,
                    CreatedAt = product.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建产品时发生错误");
                throw;
            }
        }

        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductRequest request)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    throw new KeyNotFoundException($"产品ID {id} 不存在");
                }

                product.Name = request.Name;
                product.Description = request.Description;
                product.Price = request.Price;
                product.OriginalPrice = request.OriginalPrice;
                product.StockQuantity = request.StockQuantity;
                product.CategoryId = request.CategoryId;
                product.Brand = request.Brand;
                product.VehicleModel = request.VehicleModel;
                product.YearRange = request.YearRange;
                product.ImageUrl = request.ImageUrl;
                product.IsActive = request.IsActive;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    SKU = product.SKU,
                    Price = product.Price,
                    OriginalPrice = product.OriginalPrice,
                    StockQuantity = product.StockQuantity,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category.Name,
                    Brand = product.Brand,
                    VehicleModel = product.VehicleModel,
                    YearRange = product.YearRange,
                    ImageUrl = product.ImageUrl,
                    IsActive = product.IsActive,
                    CreatedAt = product.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新产品时发生错误，产品ID: {ProductId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return false;

                // 软删除
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除产品时发生错误，产品ID: {ProductId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateProductStockAsync(int productId, int newStockQuantity)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return false;

                product.StockQuantity = newStockQuantity;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新产品库存时发生错误，产品ID: {ProductId}", productId);
                throw;
            }
        }

        public async Task<List<string>> GetBrandsAsync()
        {
            try
            {
                return await _context.Products
                    .Where(p => p.IsActive && !string.IsNullOrEmpty(p.Brand))
                    .Select(p => p.Brand)
                    .Distinct()
                    .OrderBy(b => b)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取品牌列表时发生错误");
                throw;
            }
        }

        public async Task<List<string>> GetVehicleModelsAsync()
        {
            try
            {
                return await _context.Products
                    .Where(p => p.IsActive && !string.IsNullOrEmpty(p.VehicleModel))
                    .Select(p => p.VehicleModel)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取车型列表时发生错误");
                throw;
            }
        }

        public async Task<bool> ToggleSaleStatusAsync(int productId, bool isOnSale)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return false;
                product.IsActive = isOnSale;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切换产品销售状态时发生错误，产品ID: {ProductId}", productId);
                throw;
            }
        }




    }
}