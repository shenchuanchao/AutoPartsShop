using Microsoft.EntityFrameworkCore;
using AutoPartsShop.Core.Interfaces;
using AutoPartsShop.Domain.Common;
using AutoPartsShop.Domain.Dtos;
using AutoPartsShop.Domain.Models;
using Microsoft.Extensions.Logging;
using AutoPartsShop.Infrastructure.Data;

namespace AutoPartsShop.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(AppDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedList<OrderDto>> GetOrdersAsync(OrderQuery query)
        {
            try
            {
                var ordersQuery = _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .AsQueryable();

                // 应用筛选条件
                if (!string.IsNullOrWhiteSpace(query.UserId))
                {
                    ordersQuery = ordersQuery.Where(o => o.UserId == query.UserId);
                }

                if (query.Status.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.Status == query.Status.Value);
                }

                if (query.StartDate.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.CreatedAt >= query.StartDate.Value);
                }

                if (query.EndDate.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.CreatedAt <= query.EndDate.Value);
                }

                // 应用排序
                ordersQuery = query.SortBy?.ToLower() switch
                {
                    "date" => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.CreatedAt)
                        : ordersQuery.OrderBy(o => o.CreatedAt),
                    "amount" => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.TotalAmount)
                        : ordersQuery.OrderBy(o => o.TotalAmount),
                    _ => ordersQuery.OrderByDescending(o => o.CreatedAt)
                };

                var totalCount = await ordersQuery.CountAsync();
                var orders = await ordersQuery
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(o => new OrderDto
                    {
                        Id = o.Id,
                        OrderNumber = o.OrderNumber,
                        UserId = o.UserId,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        ShippingAddress = o.ShippingAddress,
                        RecipientPhone = o.RecipientPhone,
                        RecipientName = o.RecipientName,
                        Note = o.Note,
                        CreatedAt = o.CreatedAt,
                        PaidAt = o.PaidAt,
                        ShippedAt = o.ShippedAt,
                        CompletedAt = o.CompletedAt,
                        TotalItems = o.OrderItems.Sum(oi => oi.Quantity),
                        OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                        {
                            ProductId = oi.ProductId,
                            ProductName = oi.ProductName,
                            ProductImage = oi.ProductImage,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            TotalPrice = oi.UnitPrice * oi.Quantity
                        }).ToList()
                    })
                    .ToListAsync();

                return new PagedList<OrderDto>(orders, totalCount, query.Page, query.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单列表时发生错误");
                throw;
            }
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null) return null;

                return new OrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    UserId = order.UserId,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    ShippingAddress = order.ShippingAddress,
                    RecipientPhone = order.RecipientPhone,
                    RecipientName = order.RecipientName,
                    Note = order.Note,
                    CreatedAt = order.CreatedAt,
                    PaidAt = order.PaidAt,
                    ShippedAt = order.ShippedAt,
                    CompletedAt = order.CompletedAt,
                    TotalItems = order.OrderItems.Sum(oi => oi.Quantity),
                    OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        ProductImage = oi.ProductImage,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.UnitPrice * oi.Quantity
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单详情时发生错误，订单ID: {OrderId}", id);
                throw;
            }
        }

        public async Task<OrderDto?> GetOrderByNumberAsync(string orderNumber)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

                if (order == null) return null;

                return new OrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    UserId = order.UserId,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    ShippingAddress = order.ShippingAddress,
                    RecipientPhone = order.RecipientPhone,
                    RecipientName = order.RecipientName,
                    Note = order.Note,
                    CreatedAt = order.CreatedAt,
                    PaidAt = order.PaidAt,
                    ShippedAt = order.ShippedAt,
                    CompletedAt = order.CompletedAt,
                    TotalItems = order.OrderItems.Sum(oi => oi.Quantity),
                    OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        ProductImage = oi.ProductImage,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.UnitPrice * oi.Quantity
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单详情时发生错误，订单号: {OrderNumber}", orderNumber);
                throw;
            }
        }

        public async Task<OrderDto> CreateOrderAsync(string userId, CreateOrderRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 生成订单号
                var orderNumber = GenerateOrderNumber();

                // 验证商品库存并计算总金额
                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                foreach (var item in request.Items)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId && p.IsActive);

                    if (product == null)
                    {
                        throw new KeyNotFoundException($"商品ID {item.ProductId} 不存在");
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        throw new InvalidOperationException($"商品 {product.Name} 库存不足，当前库存: {product.StockQuantity}");
                    }

                    // 扣除库存
                    product.StockQuantity -= item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;

                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        ProductImage = product.ImageUrl,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    };

                    orderItems.Add(orderItem);
                    totalAmount += product.Price * item.Quantity;
                }

                // 创建订单
                var order = new Order
                {
                    OrderNumber = orderNumber,
                    UserId = userId,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Pending,
                    ShippingAddress = request.ShippingAddress,
                    RecipientPhone = request.RecipientPhone,
                    RecipientName = request.RecipientName,
                    Note = request.Note,
                    CreatedAt = DateTime.UtcNow,
                    OrderItems = orderItems
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 返回创建的订单
                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "创建订单时发生错误，用户ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    throw new KeyNotFoundException($"订单ID {orderId} 不存在");
                }

                order.Status = status;

                // 更新时间戳
                switch (status)
                {
                    case OrderStatus.Paid:
                        order.PaidAt = DateTime.UtcNow;
                        break;
                    case OrderStatus.Shipped:
                        order.ShippedAt = DateTime.UtcNow;
                        break;
                    case OrderStatus.Completed:
                        order.CompletedAt = DateTime.UtcNow;
                        break;
                }

                await _context.SaveChangesAsync();

                return await GetOrderByIdAsync(orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新订单状态时发生错误，订单ID: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null) return false;

                if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Paid)
                {
                    throw new InvalidOperationException("只有待支付或已支付的订单可以取消");
                }

                // 恢复库存
                foreach (var orderItem in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(orderItem.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += orderItem.Quantity;
                        product.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // 更新订单状态
                order.Status = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "取消订单时发生错误，订单ID: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<List<OrderDto>> GetUserOrdersAsync(string userId)
        {
            try
            {
                var orders = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .OrderByDescending(o => o.CreatedAt)
                    .Select(o => new OrderDto
                    {
                        Id = o.Id,
                        OrderNumber = o.OrderNumber,
                        UserId = o.UserId,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        ShippingAddress = o.ShippingAddress,
                        RecipientPhone = o.RecipientPhone,
                        RecipientName = o.RecipientName,
                        Note = o.Note,
                        CreatedAt = o.CreatedAt,
                        PaidAt = o.PaidAt,
                        ShippedAt = o.ShippedAt,
                        CompletedAt = o.CompletedAt,
                        TotalItems = o.OrderItems.Sum(oi => oi.Quantity),
                        OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                        {
                            ProductId = oi.ProductId,
                            ProductName = oi.ProductName,
                            ProductImage = oi.ProductImage,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            TotalPrice = oi.UnitPrice * oi.Quantity
                        }).ToList()
                    })
                    .ToListAsync();

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户订单时发生错误，用户ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<OrderStatisticsDto> GetOrderStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var orders = await _context.Orders
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                    .ToListAsync();

                return new OrderStatisticsDto
                {
                    TotalOrders = orders.Count,
                    TotalRevenue = orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount),
                    PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                    CompletedOrders = orders.Count(o => o.Status == OrderStatus.Completed)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单统计时发生错误");
                throw;
            }
        }

        private string GenerateOrderNumber()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"ORD{timestamp}{random}";
        }
    }
}