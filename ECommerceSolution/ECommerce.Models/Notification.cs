using ECommerce.Models.Enums;

namespace ECommerce.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public bool SendEmail { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
