using ECommerce.DL;
using ECommerce.Models;
using ECommerce.Models.Enums;

namespace ECommerce.BL
{
    public class OrderBL
    {
        private readonly OrderDL _orderDL = new();
        private readonly CartDL _cartDL = new();
        private readonly ProductDL _productDL = new();
        private readonly AuditLogDL _auditDL = new();
        private readonly NotificationDL _notifDL = new();

        public (bool Success, string Message, int OrderId) PlaceOrder(int customerId, string shippingAddress, string paymentMethod)
        {
            if (string.IsNullOrWhiteSpace(shippingAddress))
                return (false, "Shipping address is required.", 0);

            var cartItems = _cartDL.GetCartItems(customerId);
            if (cartItems.Count == 0)
                return (false, "Your cart is empty.", 0);

            var orderItems = new List<OrderItem>();
            decimal total = 0;

            foreach (var ci in cartItems)
            {
                var product = _productDL.GetProductById(ci.ProductId);
                if (product == null || !product.IsActive || product.IsBanned)
                    return (false, $"Product '{ci.ProductName}' is no longer available.", 0);
                if (product.StockQuantity < ci.Quantity)
                    return (false, $"Insufficient stock for '{ci.ProductName}'. Available: {product.StockQuantity}", 0);

                var unitPrice = product.DiscountPrice ?? product.Price;
                var itemTotal = unitPrice * ci.Quantity;
                total += itemTotal;

                orderItems.Add(new OrderItem
                {
                    ProductId = ci.ProductId,
                    SellerId = product.SellerId,
                    Quantity = ci.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = itemTotal
                });
            }

            var order = new Order
            {
                CustomerId = customerId,
                TotalAmount = total,
                ShippingAddress = shippingAddress,
                PaymentMethod = paymentMethod,
                Items = orderItems
            };

            var orderId = _orderDL.CreateOrder(order);
            _cartDL.ClearCart(customerId);
            _auditDL.LogAction(customerId, "PLACE_ORDER", "orders", orderId);

            // Notify customer
            _notifDL.CreateNotification(new Notification
            {
                UserId = customerId,
                Title = "Order Placed",
                Message = $"Your order #{orderId} has been placed successfully. Total: ${total:F2}",
                Type = NotificationType.OrderUpdate,
                SendEmail = true
            });

            // Notify sellers
            var sellerIds = orderItems.Select(i => i.SellerId).Distinct();
            foreach (var sid in sellerIds)
            {
                _notifDL.CreateNotification(new Notification
                {
                    UserId = sid,
                    Title = "New Order Received",
                    Message = $"You have received a new order #{orderId}.",
                    Type = NotificationType.OrderUpdate,
                    SendEmail = true
                });
            }

            return (true, "Order placed successfully!", orderId);
        }

        public List<Order> GetOrdersByCustomer(int customerId) => _orderDL.GetOrdersByCustomer(customerId);
        public List<Order> GetOrdersBySeller(int sellerId) => _orderDL.GetOrdersBySeller(sellerId);
        public List<Order> GetAllOrders() => _orderDL.GetAllOrders();
        public Order? GetOrderById(int orderId) => _orderDL.GetOrderById(orderId);

        public (bool Success, string Message) UpdateOrderStatus(int orderId, OrderStatus newStatus, int updatedByUserId)
        {
            var order = _orderDL.GetOrderById(orderId);
            if (order == null) return (false, "Order not found.");

            _orderDL.UpdateOrderStatus(orderId, newStatus);
            _auditDL.LogAction(updatedByUserId, "UPDATE_ORDER_STATUS", "orders", orderId, $"Old: {order.Status}", $"New: {newStatus}");

            _notifDL.CreateNotification(new Notification
            {
                UserId = order.CustomerId,
                Title = "Order Status Updated",
                Message = $"Your order #{orderId} status has been updated to: {newStatus}",
                Type = NotificationType.OrderUpdate,
                SendEmail = true
            });

            return (true, $"Order status updated to {newStatus}.");
        }
    }
}
