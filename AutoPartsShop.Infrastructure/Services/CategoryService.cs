using Microsoft.EntityFrameworkCore;
using AutoPartsShop.Core.Interfaces;
using AutoPartsShop.Domain.Models;
using Microsoft.Extensions.Logging;
using AutoPartsShop.Infrastructure.Data;
using AutoPartsShop.Domain.Dtos;

namespace AutoPartsShop.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(AppDbContext context, ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            try
            {
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .Include(c => c.SubCategories.Where(sc => sc.IsActive))
                    .Include(c => c.Products.Where(p => p.IsActive))
                    .Where(c => c.ParentId == null) // 只获取顶级分类
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.Name)
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        ImageUrl = c.ImageUrl,
                        ParentId = c.ParentId,
                        SortOrder = c.SortOrder,
                        IsActive = c.IsActive,
                        ProductCount = c.Products.Count,
                        SubCategories = c.SubCategories.Select(sc => new CategoryDto
                        {
                            Id = sc.Id,
                            Name = sc.Name,
                            Description = sc.Description,
                            ImageUrl = sc.ImageUrl,
                            ParentId = sc.ParentId,
                            SortOrder = sc.SortOrder,
                            IsActive = sc.IsActive,
                            ProductCount = sc.Products.Count(p => p.IsActive)
                        }).ToList()
                    })
                    .ToListAsync();

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分类列表时发生错误");
                throw;
            }
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.SubCategories.Where(sc => sc.IsActive))
                    .Include(c => c.Products.Where(p => p.IsActive))
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

                if (category == null) return null;

                return new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    ParentId = category.ParentId,
                    ParentName = category.ParentCategory != null ? category.ParentCategory.Name : null,
                    SortOrder = category.SortOrder,
                    IsActive = category.IsActive,
                    ProductCount = category.Products.Count,
                    SubCategories = category.SubCategories.Select(sc => new CategoryDto
                    {
                        Id = sc.Id,
                        Name = sc.Name,
                        Description = sc.Description,
                        ImageUrl = sc.ImageUrl,
                        ParentId = sc.ParentId,
                        SortOrder = sc.SortOrder,
                        IsActive = sc.IsActive,
                        ProductCount = sc.Products.Count(p => p.IsActive)
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分类详情时发生错误，分类ID: {CategoryId}", id);
                throw;
            }
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
        {
            try
            {
                // 检查分类名是否已存在
                if (await _context.Categories.AnyAsync(c => c.Name == request.Name))
                {
                    throw new InvalidOperationException($"分类名称 '{request.Name}' 已存在");
                }

                var category = new Category
                {
                    Name = request.Name,
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    ParentId = request.ParentId,
                    SortOrder = request.SortOrder,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    ParentId = category.ParentId,
                    SortOrder = category.SortOrder,
                    IsActive = category.IsActive,
                    ProductCount = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建分类时发生错误");
                throw;
            }
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.ParentCategory)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    throw new KeyNotFoundException($"分类ID {id} 不存在");
                }

                // 检查分类名是否与其他分类冲突
                if (await _context.Categories.AnyAsync(c => c.Name == request.Name && c.Id != id))
                {
                    throw new InvalidOperationException($"分类名称 '{request.Name}' 已存在");
                }

                category.Name = request.Name;
                category.Description = request.Description;
                category.ImageUrl = request.ImageUrl;
                category.ParentId = request.ParentId;
                category.SortOrder = request.SortOrder;
                category.IsActive = request.IsActive;

                await _context.SaveChangesAsync();

                return new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    ParentId = category.ParentId,
                    ParentName = category.ParentCategory?.Name,
                    SortOrder = category.SortOrder,
                    IsActive = category.IsActive,
                    ProductCount = await _context.Products.CountAsync(p => p.CategoryId == id && p.IsActive)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新分类时发生错误，分类ID: {CategoryId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.SubCategories)
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null) return false;

                // 检查是否有子分类
                if (category.SubCategories.Any())
                {
                    throw new InvalidOperationException("无法删除包含子分类的分类");
                }

                // 检查是否有商品使用此分类
                if (category.Products.Any())
                {
                    throw new InvalidOperationException("无法删除已被商品使用的分类");
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除分类时发生错误，分类ID: {CategoryId}", id);
                throw;
            }
        }

        public async Task<List<CategoryDto>> GetSubCategoriesAsync(int parentId)
        {
            try
            {
                var subCategories = await _context.Categories
                    .Where(c => c.ParentId == parentId && c.IsActive)
                    .Include(c => c.Products.Where(p => p.IsActive))
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.Name)
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        ImageUrl = c.ImageUrl,
                        ParentId = c.ParentId,
                        SortOrder = c.SortOrder,
                        IsActive = c.IsActive,
                        ProductCount = c.Products.Count
                    })
                    .ToListAsync();

                return subCategories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取子分类时发生错误，父分类ID: {ParentId}", parentId);
                throw;
            }
        }
    }
}