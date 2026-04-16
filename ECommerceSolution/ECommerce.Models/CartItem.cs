namespace ECommerce.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }

        // Navigation
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public decimal TotalPrice => Price * Quantity;
    }
}
