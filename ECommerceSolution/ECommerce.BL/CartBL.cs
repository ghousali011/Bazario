using ECommerce.DL;
using ECommerce.Models;

namespace ECommerce.BL
{
    public class CartBL
    {
        private readonly CartDL _cartDL = new();

        public List<CartItem> GetCartItems(int customerId) => _cartDL.GetCartItems(customerId);

        public (bool Success, string Message) AddToCart(int customerId, int productId, int quantity = 1)
        {
            if (quantity <= 0) return (false, "Quantity must be at least 1.");
            var result = _cartDL.AddToCart(customerId, productId, quantity);
            return result ? (true, "Added to cart.") : (false, "Failed to add to cart.");
        }

        public bool UpdateQuantity(int cartItemId, int quantity) => _cartDL.UpdateQuantity(cartItemId, quantity);
        public bool RemoveFromCart(int cartItemId) => _cartDL.RemoveFromCart(cartItemId);
        public bool ClearCart(int customerId) => _cartDL.ClearCart(customerId);
    }
}
