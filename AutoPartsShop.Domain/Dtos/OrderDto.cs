using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPartsShop.Domain.Models;

namespace AutoPartsShop.Domain.Dtos
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string RecipientPhone { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TotalItems { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CreateOrderRequest
    {
        public string ShippingAddress { get; set; } = string.Empty;
        public string RecipientPhone { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public List<OrderItemRequest> Items { get; set; } = new();
    }

    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

}
