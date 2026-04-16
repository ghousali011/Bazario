using MailKit.Net.Smtp;
using MimeKit;

namespace ECommerce.Utilities
{
    public static class EmailService
    {
        public static async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(AppConfig.SmtpFromName, AppConfig.SmtpUser));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(AppConfig.SmtpHost, AppConfig.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(AppConfig.SmtpUser, AppConfig.SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> SendOtpEmailAsync(string toEmail, string otpCode, string userName)
        {
            var subject = "Your OTP Verification Code - ECommerce Store";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; padding: 20px;'>
                    <div style='max-width: 500px; margin: 0 auto; border: 1px solid #ddd; border-radius: 8px; padding: 30px;'>
                        <h2 style='color: #2c3e50; text-align: center;'>Email Verification</h2>
                        <p>Hello {userName},</p>
                        <p>Your One-Time Password (OTP) for account verification is:</p>
                        <div style='text-align: center; margin: 20px 0;'>
                            <span style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #3498db; background: #ecf0f1; padding: 15px 30px; border-radius: 8px;'>{otpCode}</span>
                        </div>
                        <p style='color: #e74c3c;'>This code expires in {AppConfig.OtpExpiryMinutes} minutes.</p>
                        <p style='color: #7f8c8d; font-size: 12px;'>If you did not request this, please ignore this email.</p>
                    </div>
                </body>
                </html>";
            return await SendEmailAsync(toEmail, subject, body);
        }

        public static async Task<bool> SendNotificationEmailAsync(string toEmail, string title, string message)
        {
            var subject = $"Notification: {title} - ECommerce Store";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; padding: 20px;'>
                    <div style='max-width: 500px; margin: 0 auto; border: 1px solid #ddd; border-radius: 8px; padding: 30px;'>
                        <h2 style='color: #2c3e50;'>{title}</h2>
                        <p>{message}</p>
                        <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                        <p style='color: #7f8c8d; font-size: 12px;'>ECommerce Store Notifications</p>
                    </div>
                </body>
                </html>";
            return await SendEmailAsync(toEmail, subject, body);
        }
    }
}
