using AutoPartsShop.Domain.Common;
using AutoPartsShop.Domain.Dtos;
using AutoPartsShop.Domain.Models;

namespace AutoPartsShop.Core.Interfaces
{
    public interface IOrderService
    {
        Task<PagedList<OrderDto>> GetOrdersAsync(OrderQuery query);
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<OrderDto?> GetOrderByNumberAsync(string orderNumber);
        Task<OrderDto> CreateOrderAsync(string userId, CreateOrderRequest request);
        Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<bool> CancelOrderAsync(int orderId);
        Task<List<OrderDto>> GetUserOrdersAsync(string userId);
        Task<OrderStatisticsDto> GetOrderStatisticsAsync(DateTime startDate, DateTime endDate);
    }

    public class OrderStatisticsDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
    }
}
