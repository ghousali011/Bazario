namespace ECommerce.Models
{
    public class OtpVerification
    {
        public int OtpId { get; set; }
        public int UserId { get; set; }
        public string OtpCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public string Purpose { get; set; } = "EmailVerification";
    }
}
