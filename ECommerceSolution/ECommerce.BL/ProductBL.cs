using ECommerce.DL;
using ECommerce.Models;
using ECommerce.Models.Enums;

namespace ECommerce.BL
{
    public class ProductBL
    {
        private readonly ProductDL _productDL = new();
        private readonly CategoryDL _categoryDL = new();
        private readonly AuditLogDL _auditDL = new();
        private readonly NotificationDL _notifDL = new();

        public (bool Success, string Message, int ProductId) AddProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
                return (false, "Product name is required.", 0);
            if (product.Price <= 0)
                return (false, "Price must be greater than 0.", 0);
            if (product.StockQuantity < 0)
                return (false, "Stock quantity cannot be negative.", 0);

            var id = _productDL.CreateProduct(product);
            _auditDL.LogAction(product.SellerId, "ADD_PRODUCT", "products", id);
            return (true, "Product added successfully.", id);
        }

        public bool UpdateProduct(Product product)
        {
            _auditDL.LogAction(product.SellerId, "UPDATE_PRODUCT", "products", product.ProductId);
            return _productDL.UpdateProduct(product);
        }

        public List<Product> GetAllActiveProducts() => _productDL.GetAllActiveProducts();
        public List<Product> GetProductsBySeller(int sellerId) => _productDL.GetProductsBySeller(sellerId);
        public List<Product> SearchProducts(string keyword) => _productDL.SearchProducts(keyword);
        public Product? GetProductById(int productId) => _productDL.GetProductById(productId);
        public List<Category> GetAllCategories() => _categoryDL.GetAllCategories();

        public int AddCategory(Category category)
        {
            _auditDL.LogAction(null, "ADD_CATEGORY", "categories", null);
            return _categoryDL.CreateCategory(category);
        }

        public (bool Success, string Message) BanProduct(int productId, string reason, int adminId)
        {
            var product = _productDL.GetProductById(productId);
            if (product == null) return (false, "Product not found.");

            _productDL.BanProduct(productId, reason);
            _auditDL.LogAction(adminId, "BAN_PRODUCT", "products", productId, null, $"Reason: {reason}");

            _notifDL.CreateNotification(new Notification
            {
                UserId = product.SellerId,
                Title = "Product Banned",
                Message = $"Your product '{product.ProductName}' has been banned. Reason: {reason}",
                Type = NotificationType.AdminAlert,
                SendEmail = true
            });

            return (true, "Product has been banned.");
        }

        public bool UnbanProduct(int productId, int adminId)
        {
            _auditDL.LogAction(adminId, "UNBAN_PRODUCT", "products", productId);
            return _productDL.UnbanProduct(productId);
        }

        public bool DeleteProduct(int productId, int sellerId)
        {
            _auditDL.LogAction(sellerId, "DELETE_PRODUCT", "products", productId);
            return _productDL.DeleteProduct(productId);
        }
    }
}
