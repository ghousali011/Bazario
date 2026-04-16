namespace ECommerce.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public int SellerId { get; set; }
        public int CategoryId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsBanned { get; set; }
        public string? BanReason { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public string? SellerName { get; set; }
        public string? CategoryName { get; set; }
    }
}
