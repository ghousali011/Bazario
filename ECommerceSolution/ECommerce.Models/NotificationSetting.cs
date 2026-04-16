namespace ECommerce.Models
{
    public class NotificationSetting
    {
        public int SettingId { get; set; }
        public int UserId { get; set; }
        public bool EmailNotifications { get; set; } = true;
        public bool OrderUpdates { get; set; } = true;
        public bool PromotionalEmails { get; set; } = true;
        public bool AccountAlerts { get; set; } = true;
        public bool ReviewNotifications { get; set; } = true;
        public bool AdminAlerts { get; set; } = true;
    }
}
