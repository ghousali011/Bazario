using ECommerce.DL;
using ECommerce.Models;
using ECommerce.Utilities;

namespace ECommerce.BL
{
    public class NotificationBL
    {
        private readonly NotificationDL _notifDL = new();
        private readonly UserDL _userDL = new();

        public async Task CreateAndSendNotification(Notification notification)
        {
            _notifDL.CreateNotification(notification);

            if (notification.SendEmail)
            {
                var settings = _notifDL.GetSettings(notification.UserId);
                if (settings?.EmailNotifications == true)
                {
                    var user = _userDL.GetUserById(notification.UserId);
                    if (user != null)
                    {
                        await EmailService.SendNotificationEmailAsync(user.Email, notification.Title, notification.Message);
                    }
                }
            }
        }

        public List<Notification> GetNotifications(int userId, bool unreadOnly = false) =>
            _notifDL.GetNotifications(userId, unreadOnly);

        public int GetUnreadCount(int userId) => _notifDL.GetUnreadCount(userId);
        public bool MarkAsRead(int notificationId) => _notifDL.MarkAsRead(notificationId);
        public bool MarkAllAsRead(int userId) => _notifDL.MarkAllAsRead(userId);

        public NotificationSetting? GetSettings(int userId) => _notifDL.GetSettings(userId);
        public bool SaveSettings(NotificationSetting settings) => _notifDL.SaveSettings(settings);

        public bool DeleteNotification(int notificationId) => _notifDL.DeleteNotification(notificationId);
    }
}
