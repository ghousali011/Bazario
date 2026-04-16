using MySql.Data.MySqlClient;
using ECommerce.Models;
using ECommerce.Models.Enums;
using System.Data;

namespace ECommerce.DL
{
    public class OrderDL
    {
        public int CreateOrder(Order order)
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                var cmd = new MySqlCommand(@"INSERT INTO orders (customer_id, total_amount, shipping_address, payment_method, status)
                                             VALUES (@cid, @total, @addr, @pay, @status); SELECT LAST_INSERT_ID();", conn, tx);
                cmd.Parameters.AddWithValue("@cid", order.CustomerId);
                cmd.Parameters.AddWithValue("@total", order.TotalAmount);
                cmd.Parameters.AddWithValue("@addr", order.ShippingAddress);
                cmd.Parameters.AddWithValue("@pay", order.PaymentMethod ?? "COD");
                cmd.Parameters.AddWithValue("@status", (int)OrderStatus.Pending);
                var orderId = Convert.ToInt32(cmd.ExecuteScalar());

                foreach (var item in order.Items)
                {
                    var itemCmd = new MySqlCommand(@"INSERT INTO order_items (order_id, product_id, seller_id, quantity, unit_price, total_price)
                                                     VALUES (@oid, @pid, @sid, @qty, @uprice, @tprice)", conn, tx);
                    itemCmd.Parameters.AddWithValue("@oid", orderId);
                    itemCmd.Parameters.AddWithValue("@pid", item.ProductId);
                    itemCmd.Parameters.AddWithValue("@sid", item.SellerId);
                    itemCmd.Parameters.AddWithValue("@qty", item.Quantity);
                    itemCmd.Parameters.AddWithValue("@uprice", item.UnitPrice);
                    itemCmd.Parameters.AddWithValue("@tprice", item.TotalPrice);
                    itemCmd.ExecuteNonQuery();

                    // Reduce stock
                    var stockCmd = new MySqlCommand("UPDATE products SET stock_quantity = stock_quantity - @qty WHERE product_id = @pid", conn, tx);
                    stockCmd.Parameters.AddWithValue("@qty", item.Quantity);
                    stockCmd.Parameters.AddWithValue("@pid", item.ProductId);
                    stockCmd.ExecuteNonQuery();
                }

                tx.Commit();
                return orderId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public List<Order> GetOrdersByCustomer(int customerId)
        {
            var query = @"SELECT o.*, u.full_name AS customer_name FROM orders o
                          JOIN users u ON o.customer_id = u.user_id
                          WHERE o.customer_id = @cid ORDER BY o.order_date DESC";
            return MapOrders(DbHelper.ExecuteQuery(query, new MySqlParameter("@cid", customerId)));
        }

        public List<Order> GetOrdersBySeller(int sellerId)
        {
            var query = @"SELECT DISTINCT o.*, u.full_name AS customer_name FROM orders o
                          JOIN users u ON o.customer_id = u.user_id
                          JOIN order_items oi ON o.order_id = oi.order_id
                          WHERE oi.seller_id = @sid ORDER BY o.order_date DESC";
            return MapOrders(DbHelper.ExecuteQuery(query, new MySqlParameter("@sid", sellerId)));
        }

        public List<Order> GetAllOrders()
        {
            var query = @"SELECT o.*, u.full_name AS customer_name FROM orders o
                          JOIN users u ON o.customer_id = u.user_id ORDER BY o.order_date DESC";
            return MapOrders(DbHelper.ExecuteQuery(query));
        }

        public Order? GetOrderById(int orderId)
        {
            var query = @"SELECT o.*, u.full_name AS customer_name FROM orders o
                          JOIN users u ON o.customer_id = u.user_id WHERE o.order_id = @oid";
            var orders = MapOrders(DbHelper.ExecuteQuery(query, new MySqlParameter("@oid", orderId)));
            var order = orders.FirstOrDefault();
            if (order != null)
            {
                order.Items = GetOrderItems(orderId);
            }
            return order;
        }

        public List<OrderItem> GetOrderItems(int orderId)
        {
            var query = @"SELECT oi.*, p.product_name, u.full_name AS seller_name
                          FROM order_items oi
                          JOIN products p ON oi.product_id = p.product_id
                          JOIN users u ON oi.seller_id = u.user_id
                          WHERE oi.order_id = @oid";
            var dt = DbHelper.ExecuteQuery(query, new MySqlParameter("@oid", orderId));
            return dt.AsEnumerable().Select(row => new OrderItem
            {
                OrderItemId = Convert.ToInt32(row["order_item_id"]),
                OrderId = Convert.ToInt32(row["order_id"]),
                ProductId = Convert.ToInt32(row["product_id"]),
                SellerId = Convert.ToInt32(row["seller_id"]),
                Quantity = Convert.ToInt32(row["quantity"]),
                UnitPrice = Convert.ToDecimal(row["unit_price"]),
                TotalPrice = Convert.ToDecimal(row["total_price"]),
                ProductName = row["product_name"].ToString(),
                SellerName = row["seller_name"].ToString()
            }).ToList();
        }

        public bool UpdateOrderStatus(int orderId, OrderStatus status)
        {
            return DbHelper.ExecuteNonQuery("UPDATE orders SET status = @status, updated_at = NOW() WHERE order_id = @oid",
                new MySqlParameter("@status", (int)status), new MySqlParameter("@oid", orderId)) > 0;
        }

        private List<Order> MapOrders(DataTable dt)
        {
            return dt.AsEnumerable().Select(row => new Order
            {
                OrderId = Convert.ToInt32(row["order_id"]),
                CustomerId = Convert.ToInt32(row["customer_id"]),
                TotalAmount = Convert.ToDecimal(row["total_amount"]),
                ShippingAddress = row["shipping_address"].ToString()!,
                PaymentMethod = row["payment_method"]?.ToString(),
                Status = (OrderStatus)Convert.ToInt32(row["status"]),
                OrderDate = Convert.ToDateTime(row["order_date"]),
                CustomerName = row.Table.Columns.Contains("customer_name") ? row["customer_name"]?.ToString() : null
            }).ToList();
        }
    }
}
