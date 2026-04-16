namespace ECommerce.Utilities
{
    public static class AppConfig
    {
        // Database
        public static string DbServer { get; set; } = "192.168.100.10";
        public static string DbPort { get; set; } = "3306";
        public static string DbName { get; set; } = "ecommerce_db";
        public static string DbUser { get; set; } = "Bazario";
        public static string DbPassword { get; set; } = "bazario.123";

        public static string ConnectionString =>
            $"Server={DbServer};Port={DbPort};Database={DbName};Uid={DbUser};Pwd={DbPassword};SslMode=Preferred;";

        // SMTP
        public static string SmtpHost { get; set; } = "smtp.gmail.com";
        public static int SmtpPort { get; set; } = 587;
        public static string SmtpUser { get; set; } = "your-email@gmail.com";
        public static string SmtpPassword { get; set; } = "your-app-password";
        public static string SmtpFromName { get; set; } = "ECommerce Store";

        // OTP
        public static int OtpLength { get; set; } = 6;
        public static int OtpExpiryMinutes { get; set; } = 10;

        // Backup
        public static string BackupDirectory { get; set; } = @"C:\ECommerceBackups";
    }
}
