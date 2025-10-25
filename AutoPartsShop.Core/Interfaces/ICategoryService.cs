
namespace AutoPartsShop.Core.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request);
        Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
        Task<bool> DeleteCategoryAsync(int id);
        Task<List<CategoryDto>> GetSubCategoriesAsync(int parentId);
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
        public List<CategoryDto> SubCategories { get; set; } = new();
    }

    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
    }

    public class UpdateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}