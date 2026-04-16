using ECommerce.DL;
using ECommerce.Models;
using ECommerce.Models.Enums;

namespace ECommerce.BL
{
    public class ReviewBL
    {
        private readonly ReviewDL _reviewDL = new();
        private readonly OrderDL _orderDL = new();
        private readonly AuditLogDL _auditDL = new();
        private readonly NotificationDL _notifDL = new();
        private readonly ProductDL _productDL = new();

        public (bool Success, string Message) AddReview(int customerId, int productId, int orderId, int rating, string? comment)
        {
            if (rating < 1 || rating > 5)
                return (false, "Rating must be between 1 and 5.");

            // Verify customer ordered this product
            var order = _orderDL.GetOrderById(orderId);
            if (order == null || order.CustomerId != customerId)
                return (false, "You can only review products you have purchased.");
            if (order.Status != OrderStatus.Delivered)
                return (false, "You can only review after the order is delivered.");

            if (_reviewDL.HasReviewed(customerId, productId, orderId))
                return (false, "You have already reviewed this product for this order.");

            _reviewDL.CreateReview(new Review
            {
                ProductId = productId,
                CustomerId = customerId,
                OrderId = orderId,
                Rating = rating,
                Comment = comment
            });

            _auditDL.LogAction(customerId, "ADD_REVIEW", "reviews", productId);

            // Notify seller
            var product = _productDL.GetProductById(productId);
            if (product != null)
            {
                _notifDL.CreateNotification(new Notification
                {
                    UserId = product.SellerId,
                    Title = "New Review",
                    Message = $"Your product '{product.ProductName}' received a {rating}-star review.",
                    Type = NotificationType.Review,
                    SendEmail = true
                });
            }

            return (true, "Review submitted successfully.");
        }

        public List<Review> GetProductReviews(int productId) => _reviewDL.GetReviewsByProduct(productId);
        public List<Review> GetCustomerReviews(int customerId) => _reviewDL.GetReviewsByCustomer(customerId);
    }
}
