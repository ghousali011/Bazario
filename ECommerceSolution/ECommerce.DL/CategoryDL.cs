using MySql.Data.MySqlClient;
using ECommerce.Models;
using System.Data;

namespace ECommerce.DL
{
    public class CategoryDL
    {
        public List<Category> GetAllCategories()
        {
            var dt = DbHelper.ExecuteQuery("SELECT * FROM categories WHERE is_active = 1 ORDER BY category_name");
            return dt.AsEnumerable().Select(row => new Category
            {
                CategoryId = Convert.ToInt32(row["category_id"]),
                CategoryName = row["category_name"].ToString()!,
                Description = row["description"]?.ToString(),
                ParentCategoryId = row["parent_category_id"] == DBNull.Value ? null : Convert.ToInt32(row["parent_category_id"]),
                IsActive = Convert.ToBoolean(row["is_active"])
            }).ToList();
        }

        public int CreateCategory(Category category)
        {
            var query = @"INSERT INTO categories (category_name, description, parent_category_id)
                          VALUES (@name, @desc, @parent); SELECT LAST_INSERT_ID();";
            var result = DbHelper.ExecuteScalar(query,
                new MySqlParameter("@name", category.CategoryName),
                new MySqlParameter("@desc", (object?)category.Description ?? DBNull.Value),
                new MySqlParameter("@parent", (object?)category.ParentCategoryId ?? DBNull.Value));
            return Convert.ToInt32(result);
        }

        public bool UpdateCategory(Category category)
        {
            return DbHelper.ExecuteNonQuery("UPDATE categories SET category_name=@name, description=@desc WHERE category_id=@id",
                new MySqlParameter("@name", category.CategoryName),
                new MySqlParameter("@desc", (object?)category.Description ?? DBNull.Value),
                new MySqlParameter("@id", category.CategoryId)) > 0;
        }

        public bool DeleteCategory(int categoryId)
        {
            return DbHelper.ExecuteNonQuery("UPDATE categories SET is_active = 0 WHERE category_id = @id",
                new MySqlParameter("@id", categoryId)) > 0;
        }
    }
}
