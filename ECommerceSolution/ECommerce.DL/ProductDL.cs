using MySql.Data.MySqlClient;
using ECommerce.Models;
using System.Data;

namespace ECommerce.DL
{
    public class ProductDL
    {
        public int CreateProduct(Product product)
        {
            var query = @"INSERT INTO products (seller_id, category_id, product_name, description, price, discount_price, stock_quantity, image_url)
                          VALUES (@sid, @cid, @name, @desc, @price, @disc, @stock, @img); SELECT LAST_INSERT_ID();";
            var result = DbHelper.ExecuteScalar(query,
                new MySqlParameter("@sid", product.SellerId),
                new MySqlParameter("@cid", product.CategoryId),
                new MySqlParameter("@name", product.ProductName),
                new MySqlParameter("@desc", (object?)product.Description ?? DBNull.Value),
                new MySqlParameter("@price", product.Price),
                new MySqlParameter("@disc", (object?)product.DiscountPrice ?? DBNull.Value),
                new MySqlParameter("@stock", product.StockQuantity),
                new MySqlParameter("@img", (object?)product.ImageUrl ?? DBNull.Value));
            return Convert.ToInt32(result);
        }

        public bool UpdateProduct(Product product)
        {
            var query = @"UPDATE products SET category_id=@cid, product_name=@name, description=@desc,
                          price=@price, discount_price=@disc, stock_quantity=@stock, image_url=@img, updated_at=NOW()
                          WHERE product_id=@pid";
            return DbHelper.ExecuteNonQuery(query,
                new MySqlParameter("@cid", product.CategoryId),
                new MySqlParameter("@name", product.ProductName),
                new MySqlParameter("@desc", (object?)product.Description ?? DBNull.Value),
                new MySqlParameter("@price", product.Price),
                new MySqlParameter("@disc", (object?)product.DiscountPrice ?? DBNull.Value),
                new MySqlParameter("@stock", product.StockQuantity),
                new MySqlParameter("@img", (object?)product.ImageUrl ?? DBNull.Value),
                new MySqlParameter("@pid", product.ProductId)) > 0;
        }

        public List<Product> GetAllActiveProducts()
        {
            var query = @"SELECT p.*, u.full_name AS seller_name, c.category_name
                          FROM products p
                          JOIN users u ON p.seller_id = u.user_id
                          JOIN categories c ON p.category_id = c.category_id
                          WHERE p.is_active = 1 AND p.is_banned = 0 ORDER BY p.created_at DESC";
            return MapProducts(DbHelper.ExecuteQuery(query));
        }

        public List<Product> GetProductsBySeller(int sellerId)
        {
            var query = @"SELECT p.*, u.full_name AS seller_name, c.category_name
                          FROM products p
                          JOIN users u ON p.seller_id = u.user_id
                          JOIN categories c ON p.category_id = c.category_id
                          WHERE p.seller_id = @sid AND p.is_active = 1 ORDER BY p.created_at DESC";
            return MapProducts(DbHelper.ExecuteQuery(query, new MySqlParameter("@sid", sellerId)));
        }

        public List<Product> SearchProducts(string keyword)
        {
            var query = @"SELECT p.*, u.full_name AS seller_name, c.category_name
                          FROM products p
                          JOIN users u ON p.seller_id = u.user_id
                          JOIN categories c ON p.category_id = c.category_id
                          WHERE p.is_active = 1 AND p.is_banned = 0
                          AND (p.product_name LIKE @kw OR p.description LIKE @kw OR c.category_name LIKE @kw)
                          ORDER BY p.created_at DESC";
            return MapProducts(DbHelper.ExecuteQuery(query, new MySqlParameter("@kw", $"%{keyword}%")));
        }

        public Product? GetProductById(int productId)
        {
            var query = @"SELECT p.*, u.full_name AS seller_name, c.category_name
                          FROM products p
                          JOIN users u ON p.seller_id = u.user_id
                          JOIN categories c ON p.category_id = c.category_id
                          WHERE p.product_id = @pid";
            var list = MapProducts(DbHelper.ExecuteQuery(query, new MySqlParameter("@pid", productId)));
            return list.FirstOrDefault();
        }

        public bool BanProduct(int productId, string reason)
        {
            return DbHelper.ExecuteNonQuery("UPDATE products SET is_banned = 1, ban_reason = @reason WHERE product_id = @id",
                new MySqlParameter("@id", productId), new MySqlParameter("@reason", reason)) > 0;
        }

        public bool UnbanProduct(int productId)
        {
            return DbHelper.ExecuteNonQuery("UPDATE products SET is_banned = 0, ban_reason = NULL WHERE product_id = @id",
                new MySqlParameter("@id", productId)) > 0;
        }

        public bool DeleteProduct(int productId)
        {
            return DbHelper.ExecuteNonQuery("UPDATE products SET is_active = 0 WHERE product_id = @id",
                new MySqlParameter("@id", productId)) > 0;
        }

        private List<Product> MapProducts(DataTable dt)
        {
            return dt.AsEnumerable().Select(row => new Product
            {
                ProductId = Convert.ToInt32(row["product_id"]),
                SellerId = Convert.ToInt32(row["seller_id"]),
                CategoryId = Convert.ToInt32(row["category_id"]),
                ProductName = row["product_name"].ToString()!,
                Description = row["description"]?.ToString(),
                Price = Convert.ToDecimal(row["price"]),
                DiscountPrice = row["discount_price"] == DBNull.Value ? null : Convert.ToDecimal(row["discount_price"]),
                StockQuantity = Convert.ToInt32(row["stock_quantity"]),
                ImageUrl = row["image_url"]?.ToString(),
                IsActive = Convert.ToBoolean(row["is_active"]),
                IsBanned = Convert.ToBoolean(row["is_banned"]),
                BanReason = row["ban_reason"] == DBNull.Value ? null : row["ban_reason"].ToString(),
                AverageRating = row["average_rating"] == DBNull.Value ? 0 : Convert.ToDouble(row["average_rating"]),
                TotalReviews = Convert.ToInt32(row["total_reviews"]),
                CreatedAt = Convert.ToDateTime(row["created_at"]),
                SellerName = row.Table.Columns.Contains("seller_name") ? row["seller_name"]?.ToString() : null,
                CategoryName = row.Table.Columns.Contains("category_name") ? row["category_name"]?.ToString() : null
            }).ToList();
        }
    }
}
