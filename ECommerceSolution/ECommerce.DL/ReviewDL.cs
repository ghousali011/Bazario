using MySql.Data.MySqlClient;
using ECommerce.Models;
using System.Data;

namespace ECommerce.DL
{
    public class ReviewDL
    {
        public int CreateReview(Review review)
        {
            var query = @"INSERT INTO reviews (product_id, customer_id, order_id, rating, comment)
                          VALUES (@pid, @cid, @oid, @rating, @comment); SELECT LAST_INSERT_ID();";
            var result = DbHelper.ExecuteScalar(query,
                new MySqlParameter("@pid", review.ProductId),
                new MySqlParameter("@cid", review.CustomerId),
                new MySqlParameter("@oid", review.OrderId),
                new MySqlParameter("@rating", review.Rating),
                new MySqlParameter("@comment", (object?)review.Comment ?? DBNull.Value));
            var reviewId = Convert.ToInt32(result);

            // Update product average rating
            DbHelper.ExecuteNonQuery(@"UPDATE products SET
                average_rating = (SELECT AVG(rating) FROM reviews WHERE product_id = @pid),
                total_reviews = (SELECT COUNT(*) FROM reviews WHERE product_id = @pid)
                WHERE product_id = @pid", new MySqlParameter("@pid", review.ProductId));

            return reviewId;
        }

        public List<Review> GetReviewsByProduct(int productId)
        {
            var query = @"SELECT r.*, u.full_name AS customer_name FROM reviews r
                          JOIN users u ON r.customer_id = u.user_id
                          WHERE r.product_id = @pid ORDER BY r.created_at DESC";
            var dt = DbHelper.ExecuteQuery(query, new MySqlParameter("@pid", productId));
            return MapReviews(dt);
        }

        public List<Review> GetReviewsByCustomer(int customerId)
        {
            var query = @"SELECT r.*, u.full_name AS customer_name, p.product_name FROM reviews r
                          JOIN users u ON r.customer_id = u.user_id
                          JOIN products p ON r.product_id = p.product_id
                          WHERE r.customer_id = @cid ORDER BY r.created_at DESC";
            var dt = DbHelper.ExecuteQuery(query, new MySqlParameter("@cid", customerId));
            return MapReviews(dt);
        }

        public bool HasReviewed(int customerId, int productId, int orderId)
        {
            var result = DbHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM reviews WHERE customer_id = @cid AND product_id = @pid AND order_id = @oid",
                new MySqlParameter("@cid", customerId), new MySqlParameter("@pid", productId), new MySqlParameter("@oid", orderId));
            return Convert.ToInt32(result) > 0;
        }

        public bool DeleteReview(int reviewId)
        {
            return DbHelper.ExecuteNonQuery("DELETE FROM reviews WHERE review_id = @id",
                new MySqlParameter("@id", reviewId)) > 0;
        }

        private List<Review> MapReviews(DataTable dt)
        {
            return dt.AsEnumerable().Select(row => new Review
            {
                ReviewId = Convert.ToInt32(row["review_id"]),
                ProductId = Convert.ToInt32(row["product_id"]),
                CustomerId = Convert.ToInt32(row["customer_id"]),
                OrderId = Convert.ToInt32(row["order_id"]),
                Rating = Convert.ToInt32(row["rating"]),
                Comment = row["comment"]?.ToString(),
                CreatedAt = Convert.ToDateTime(row["created_at"]),
                CustomerName = row.Table.Columns.Contains("customer_name") ? row["customer_name"]?.ToString() : null,
                ProductName = row.Table.Columns.Contains("product_name") ? row["product_name"]?.ToString() : null
            }).ToList();
        }
    }
}
