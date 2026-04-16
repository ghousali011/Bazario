namespace ECommerce.Models
{
    public class AdminRoleRequest
    {
        public int RequestId { get; set; }
        public int RequesterId { get; set; }
        public int? ApprovedById { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Expired
        public DateTime? TimeLimit { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // Navigation
        public string? RequesterName { get; set; }
        public string? ApprovedByName { get; set; }
    }
}
