namespace ECommerce.Models
{
    public class AuditLog
    {
        public long LogId { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public int? RecordId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public string? UserName { get; set; }
    }
}
