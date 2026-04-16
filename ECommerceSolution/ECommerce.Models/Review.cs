namespace ECommerce.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public string? CustomerName { get; set; }
        public string? ProductName { get; set; }
    }
}
