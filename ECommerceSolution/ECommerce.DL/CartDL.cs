using MySql.Data.MySqlClient;
using ECommerce.Models;
using System.Data;

namespace ECommerce.DL
{
    public class CartDL
    {
        public List<CartItem> GetCartItems(int customerId)
        {
            var query = @"SELECT c.*, p.product_name, p.price, p.image_url
                          FROM cart_items c
                          JOIN products p ON c.product_id = p.product_id
                          WHERE c.customer_id = @cid ORDER BY c.added_at DESC";
            var dt = DbHelper.ExecuteQuery(query, new MySqlParameter("@cid", customerId));
            return dt.AsEnumerable().Select(row => new CartItem
            {
                CartItemId = Convert.ToInt32(row["cart_item_id"]),
                CustomerId = Convert.ToInt32(row["customer_id"]),
                ProductId = Convert.ToInt32(row["product_id"]),
                Quantity = Convert.ToInt32(row["quantity"]),
                AddedAt = Convert.ToDateTime(row["added_at"]),
                ProductName = row["product_name"].ToString()!,
                Price = Convert.ToDecimal(row["price"]),
                ImageUrl = row["image_url"]?.ToString()
            }).ToList();
        }

        public bool AddToCart(int customerId, int productId, int quantity)
        {
            // Check if already in cart
            var existing = DbHelper.ExecuteScalar(
                "SELECT cart_item_id FROM cart_items WHERE customer_id = @cid AND product_id = @pid",
                new MySqlParameter("@cid", customerId), new MySqlParameter("@pid", productId));

            if (existing != null)
            {
                return DbHelper.ExecuteNonQuery(
                    "UPDATE cart_items SET quantity = quantity + @qty WHERE customer_id = @cid AND product_id = @pid",
                    new MySqlParameter("@qty", quantity), new MySqlParameter("@cid", customerId), new MySqlParameter("@pid", productId)) > 0;
            }

            return DbHelper.ExecuteNonQuery(
                "INSERT INTO cart_items (customer_id, product_id, quantity) VALUES (@cid, @pid, @qty)",
                new MySqlParameter("@cid", customerId), new MySqlParameter("@pid", productId), new MySqlParameter("@qty", quantity)) > 0;
        }

        public bool UpdateQuantity(int cartItemId, int quantity)
        {
            return DbHelper.ExecuteNonQuery("UPDATE cart_items SET quantity = @qty WHERE cart_item_id = @id",
                new MySqlParameter("@qty", quantity), new MySqlParameter("@id", cartItemId)) > 0;
        }

        public bool RemoveFromCart(int cartItemId)
        {
            return DbHelper.ExecuteNonQuery("DELETE FROM cart_items WHERE cart_item_id = @id",
                new MySqlParameter("@id", cartItemId)) > 0;
        }

        public bool ClearCart(int customerId)
        {
            return DbHelper.ExecuteNonQuery("DELETE FROM cart_items WHERE customer_id = @cid",
                new MySqlParameter("@cid", customerId)) > 0;
        }
    }
}
