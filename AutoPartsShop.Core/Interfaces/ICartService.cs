using AutoPartsShop.Domain.Dtos;

namespace AutoPartsShop.Core.Interfaces
{
    public interface ICartService
    {
        Task<ShoppingCartDto> GetCartAsync(string userId);
        Task<ShoppingCartDto> AddToCartAsync(string userId, AddToCartRequest request);
        Task<ShoppingCartDto> UpdateCartItemAsync(string userId, int cartItemId, UpdateCartItemRequest request);
        Task<bool> RemoveFromCartAsync(string userId, int cartItemId);
        Task<bool> ClearCartAsync(string userId);
        Task<int> GetCartItemCountAsync(string userId);
    }
}