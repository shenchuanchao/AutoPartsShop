using Microsoft.EntityFrameworkCore;
using AutoPartsShop.Core.Interfaces;
using AutoPartsShop.Domain.Dtos;
using AutoPartsShop.Domain.Models;
using Microsoft.Extensions.Logging;
using AutoPartsShop.Infrastructure.Data;

namespace AutoPartsShop.Infrastructure.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(AppDbContext context, ILogger<CartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ShoppingCartDto> GetCartAsync(string userId)
        {
            try
            {
                var cart = await _context.ShoppingCarts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    // 如果购物车不存在，创建一个新的
                    cart = new ShoppingCart
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.ShoppingCarts.Add(cart);
                    await _context.SaveChangesAsync();
                }

                return MapToCartDto(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取购物车时发生错误，用户ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<ShoppingCartDto> AddToCartAsync(string userId, AddToCartRequest request)
        {
            try
            {
                var cart = await _context.ShoppingCarts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.ShoppingCarts.Add(cart);
                }

                // 检查商品是否存在且有库存
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive);

                if (product == null)
                {
                    throw new KeyNotFoundException($"商品ID {request.ProductId} 不存在");
                }

                if (product.StockQuantity < request.Quantity)
                {
                    throw new InvalidOperationException($"商品 {product.Name} 库存不足，当前库存: {product.StockQuantity}");
                }

                // 检查是否已存在购物车项
                var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);

                if (existingItem != null)
                {
                    // 更新数量
                    existingItem.Quantity += request.Quantity;
                }
                else
                {
                    // 添加新项
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        AddedAt = DateTime.UtcNow
                    };
                    cart.CartItems.Add(cartItem);
                }

                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return MapToCartDto(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加商品到购物车时发生错误，用户ID: {UserId}, 商品ID: {ProductId}", userId, request.ProductId);
                throw;
            }
        }

        public async Task<ShoppingCartDto> UpdateCartItemAsync(string userId, int cartItemId, UpdateCartItemRequest request)
        {
            try
            {
                var cart = await _context.ShoppingCarts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    throw new KeyNotFoundException($"用户 {userId} 的购物车不存在");
                }

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
                if (cartItem == null)
                {
                    throw new KeyNotFoundException($"购物车项ID {cartItemId} 不存在");
                }

                // 检查库存
                if (cartItem.Product.StockQuantity < request.Quantity)
                {
                    throw new InvalidOperationException($"商品 {cartItem.Product.Name} 库存不足，当前库存: {cartItem.Product.StockQuantity}");
                }

                cartItem.Quantity = request.Quantity;
                cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return MapToCartDto(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新购物车项时发生错误，用户ID: {UserId}, 购物车项ID: {CartItemId}", userId, cartItemId);
                throw;
            }
        }

        public async Task<bool> RemoveFromCartAsync(string userId, int cartItemId)
        {
            try
            {
                var cart = await _context.ShoppingCarts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null) return false;

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
                if (cartItem == null) return false;

                cart.CartItems.Remove(cartItem);
                cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从购物车移除商品时发生错误，用户ID: {UserId}, 购物车项ID: {CartItemId}", userId, cartItemId);
                throw;
            }
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            try
            {
                var cart = await _context.ShoppingCarts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null) return false;

                cart.CartItems.Clear();
                cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空购物车时发生错误，用户ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            try
            {
                var cart = await _context.ShoppingCarts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                return cart?.CartItems.Sum(ci => ci.Quantity) ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取购物车商品数量时发生错误，用户ID: {UserId}", userId);
                throw;
            }
        }

        private ShoppingCartDto MapToCartDto(ShoppingCart cart)
        {
            return new ShoppingCartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                TotalItems = cart.CartItems.Sum(ci => ci.Quantity),
                TotalPrice = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity),
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    ProductImage = ci.Product.ImageUrl,
                    Price = ci.Product.Price,
                    Quantity = ci.Quantity,
                    TotalPrice = ci.Product.Price * ci.Quantity,
                    StockQuantity = ci.Product.StockQuantity
                }).ToList()
            };
        }
    }
}