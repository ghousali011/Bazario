using MySql.Data.MySqlClient;
using ECommerce.Models;
using ECommerce.Models.Enums;
using System.Data;

namespace ECommerce.DL
{
    public class UserDL
    {
        public int CreateUser(User user)
        {
            var query = @"INSERT INTO users (full_name, email, password_hash, phone, role, is_email_verified, is_banned, is_active)
                          VALUES (@name, @email, @pass, @phone, @role, @verified, 0, 1);
                          SELECT LAST_INSERT_ID();";
            var result = DbHelper.ExecuteScalar(query,
                new MySqlParameter("@name", user.FullName),
                new MySqlParameter("@email", user.Email),
                new MySqlParameter("@pass", user.PasswordHash),
                new MySqlParameter("@phone", user.Phone),
                new MySqlParameter("@role", (int)user.Role),
                new MySqlParameter("@verified", user.IsEmailVerified));
            return Convert.ToInt32(result);
        }

        public User? GetUserByEmail(string email)
        {
            var query = "SELECT * FROM users WHERE email = @email";
            var dt = DbHelper.ExecuteQuery(query, new MySqlParameter("@email", email));
            if (dt.Rows.Count == 0) return null;
            return MapUser(dt.Rows[0]);
        }

        public User? GetUserById(int userId)
        {
            var query = "SELECT * FROM users WHERE user_id = @id";
            var dt = DbHelper.ExecuteQuery(query, new MySqlParameter("@id", userId));
            if (dt.Rows.Count == 0) return null;
            return MapUser(dt.Rows[0]);
        }

        public List<User> GetAllUsers()
        {
            var dt = DbHelper.ExecuteQuery("SELECT * FROM users ORDER BY created_at DESC");
            return dt.AsEnumerable().Select(MapUser).ToList();
        }

        public List<User> GetUsersByRole(UserRole role)
        {
            var dt = DbHelper.ExecuteQuery("SELECT * FROM users WHERE role = @role ORDER BY created_at DESC",
                new MySqlParameter("@role", (int)role));
            return dt.AsEnumerable().Select(MapUser).ToList();
        }

        public bool UpdateUser(User user)
        {
            var query = @"UPDATE users SET full_name=@name, phone=@phone, profile_image_url=@img, updated_at=NOW() WHERE user_id=@id";
            return DbHelper.ExecuteNonQuery(query,
                new MySqlParameter("@name", user.FullName),
                new MySqlParameter("@phone", user.Phone),
                new MySqlParameter("@img", (object?)user.ProfileImageUrl ?? DBNull.Value),
                new MySqlParameter("@id", user.UserId)) > 0;
        }

        public bool VerifyEmail(int userId)
        {
            return DbHelper.ExecuteNonQuery("UPDATE users SET is_email_verified = 1 WHERE user_id = @id",
                new MySqlParameter("@id", userId)) > 0;
        }

        public bool BanUser(int userId, string reason)
        {
            return DbHelper.ExecuteNonQuery("UPDATE users SET is_banned = 1, ban_reason = @reason WHERE user_id = @id",
                new MySqlParameter("@id", userId), new MySqlParameter("@reason", reason)) > 0;
        }

        public bool UnbanUser(int userId)
        {
            return DbHelper.ExecuteNonQuery("UPDATE users SET is_banned = 0, ban_reason = NULL WHERE user_id = @id",
                new MySqlParameter("@id", userId)) > 0;
        }

        public bool UpdateLastLogin(int userId)
        {
            return DbHelper.ExecuteNonQuery("UPDATE users SET last_login_at = NOW() WHERE user_id = @id",
                new MySqlParameter("@id", userId)) > 0;
        }

        public bool UpdateRole(int userId, UserRole role)
        {
            return DbHelper.ExecuteNonQuery("UPDATE users SET role = @role WHERE user_id = @id",
                new MySqlParameter("@id", userId), new MySqlParameter("@role", (int)role)) > 0;
        }

        public bool EmailExists(string email)
        {
            var result = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM users WHERE email = @email",
                new MySqlParameter("@email", email));
            return Convert.ToInt32(result) > 0;
        }

        private User MapUser(DataRow row)
        {
            return new User
            {
                UserId = Convert.ToInt32(row["user_id"]),
                FullName = row["full_name"].ToString()!,
                Email = row["email"].ToString()!,
                PasswordHash = row["password_hash"].ToString()!,
                Phone = row["phone"]?.ToString() ?? "",
                Role = (UserRole)Convert.ToInt32(row["role"]),
                IsEmailVerified = Convert.ToBoolean(row["is_email_verified"]),
                IsBanned = Convert.ToBoolean(row["is_banned"]),
                BanReason = row["ban_reason"] == DBNull.Value ? null : row["ban_reason"].ToString(),
                IsActive = Convert.ToBoolean(row["is_active"]),
                CreatedAt = Convert.ToDateTime(row["created_at"]),
                UpdatedAt = row["updated_at"] == DBNull.Value ? null : Convert.ToDateTime(row["updated_at"]),
                LastLoginAt = row["last_login_at"] == DBNull.Value ? null : Convert.ToDateTime(row["last_login_at"]),
                ProfileImageUrl = row["profile_image_url"] == DBNull.Value ? null : row["profile_image_url"].ToString()
            };
        }
    }
}
