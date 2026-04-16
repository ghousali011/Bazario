using MySql.Data.MySqlClient;
using ECommerce.Models;
using ECommerce.Models.Enums;
using System.Data;

namespace ECommerce.DL
{
    public class NotificationDL
    {
        public int CreateNotification(Notification notification)
        {
            var query = @"INSERT INTO notifications (user_id, title, message, type, send_email)
                          VALUES (@uid, @title, @msg, @type, @email); SELECT LAST_INSERT_ID();";
            var result = DbHelper.ExecuteScalar(query,
                new MySqlParameter("@uid", notification.UserId),
                new MySqlParameter("@title", notification.Title),
                new MySqlParameter("@msg", notification.Message),
                new MySqlParameter("@type", (int)notification.Type),
                new MySqlParameter("@email", notification.SendEmail));
            return Convert.ToInt32(result);
        }

        public List<Notification> GetNotifications(int userId, bool unreadOnly = false)
        {
            var query = unreadOnly
                ? "SELECT * FROM notifications WHERE user_id = @uid AND is_read = 0 ORDER BY created_at DESC"
                : "SELECT * FROM notifications WHERE user_id = @uid ORDER BY created_at DESC LIMIT 50";
            var dt = DbHelper.ExecuteQuery(query, new MySqlParameter("@uid", userId));
            return dt.AsEnumerable().Select(row => new Notification
            {
                NotificationId = Convert.ToInt32(row["notification_id"]),
                UserId = Convert.ToInt32(row["user_id"]),
                Title = row["title"].ToString()!,
                Message = row["message"].ToString()!,
                Type = (NotificationType)Convert.ToInt32(row["type"]),
                IsRead = Convert.ToBoolean(row["is_read"]),
                CreatedAt = Convert.ToDateTime(row["created_at"])
            }).ToList();
        }

        public int GetUnreadCount(int userId)
        {
            var result = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM notifications WHERE user_id = @uid AND is_read = 0",
                new MySqlParameter("@uid", userId));
            return Convert.ToInt32(result);
        }

        public bool MarkAsRead(int notificationId)
        {
            return DbHelper.ExecuteNonQuery("UPDATE notifications SET is_read = 1 WHERE notification_id = @id",
                new MySqlParameter("@id", notificationId)) > 0;
        }

        public bool MarkAllAsRead(int userId)
        {
            return DbHelper.ExecuteNonQuery("UPDATE notifications SET is_read = 1 WHERE user_id = @uid AND is_read = 0",
                new MySqlParameter("@uid", userId)) > 0;
        }

        // Notification Settings
        public NotificationSetting? GetSettings(int userId)
        {
            var dt = DbHelper.ExecuteQuery("SELECT * FROM notification_settings WHERE user_id = @uid",
                new MySqlParameter("@uid", userId));
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new NotificationSetting
            {
                SettingId = Convert.ToInt32(row["setting_id"]),
                UserId = Convert.ToInt32(row["user_id"]),
                EmailNotifications = Convert.ToBoolean(row["email_notifications"]),
                OrderUpdates = Convert.ToBoolean(row["order_updates"]),
                PromotionalEmails = Convert.ToBoolean(row["promotional_emails"]),
                AccountAlerts = Convert.ToBoolean(row["account_alerts"]),
                ReviewNotifications = Convert.ToBoolean(row["review_notifications"]),
                AdminAlerts = Convert.ToBoolean(row["admin_alerts"])
            };
        }

        public bool SaveSettings(NotificationSetting settings)
        {
            var query = @"INSERT INTO notification_settings (user_id, email_notifications, order_updates, promotional_emails, account_alerts, review_notifications, admin_alerts)
                          VALUES (@uid, @email, @order, @promo, @account, @review, @admin)
                          ON DUPLICATE KEY UPDATE email_notifications=@email, order_updates=@order, promotional_emails=@promo,
                          account_alerts=@account, review_notifications=@review, admin_alerts=@admin";
            return DbHelper.ExecuteNonQuery(query,
                new MySqlParameter("@uid", settings.UserId),
                new MySqlParameter("@email", settings.EmailNotifications),
                new MySqlParameter("@order", settings.OrderUpdates),
                new MySqlParameter("@promo", settings.PromotionalEmails),
                new MySqlParameter("@account", settings.AccountAlerts),
                new MySqlParameter("@review", settings.ReviewNotifications),
                new MySqlParameter("@admin", settings.AdminAlerts)) > 0;
        }
        public bool DeleteNotification(int notificationId)
        {
            return DbHelper.ExecuteNonQuery("DELETE FROM notifications WHERE notification_id = @id",
                new MySqlParameter("@id", notificationId)) > 0;
        }
    }
}
