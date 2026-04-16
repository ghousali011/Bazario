using ECommerce.Models.Enums;

namespace ECommerce.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public string? CustomerName { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }
}
