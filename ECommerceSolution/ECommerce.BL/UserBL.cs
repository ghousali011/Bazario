using ECommerce.DL;
using ECommerce.Models;
using ECommerce.Models.Enums;
using ECommerce.Utilities;

namespace ECommerce.BL
{
    public class UserBL
    {
        private readonly UserDL _userDL = new();
        private readonly OtpDL _otpDL = new();
        private readonly AuditLogDL _auditDL = new();
        private readonly NotificationDL _notifDL = new();

        public (bool Success, string Message, int UserId) Register(string fullName, string email, string password, string confirmPassword, string phone, UserRole role)
        {
            var validation = ValidationHelper.ValidateRegistration(fullName, email, password, confirmPassword, phone);
            if (!validation.IsValid) return (false, validation.Message, 0);

            if (_userDL.EmailExists(email))
                return (false, "An account with this email already exists.", 0);

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(password),
                Phone = phone,
                Role = role,
                IsEmailVerified = false
            };

            var userId = _userDL.CreateUser(user);
            _auditDL.LogAction(userId, "REGISTER", "users", userId, null, $"Role: {role}");

            // Generate and send OTP
            var otpCode = OtpGenerator.GenerateOtp(AppConfig.OtpLength);
            var otp = new OtpVerification
            {
                UserId = userId,
                OtpCode = otpCode,
                Email = email,
                ExpiresAt = DateTime.Now.AddMinutes(AppConfig.OtpExpiryMinutes),
                Purpose = "EmailVerification"
            };
            _otpDL.CreateOtp(otp);

            _ = Task.Run(async () => await EmailService.SendOtpEmailAsync(email, otpCode, fullName));

            return (true, "Account created. Please verify your email with the OTP sent.", userId);
        }

        public (bool Success, string Message) VerifyOtp(string email, string otpCode)
        {
            var otp = _otpDL.GetValidOtp(email, otpCode, "EmailVerification");
            if (otp == null)
                return (false, "Invalid or expired OTP. Please request a new one.");

            _otpDL.MarkOtpUsed(otp.OtpId);
            _userDL.VerifyEmail(otp.UserId);
            _auditDL.LogAction(otp.UserId, "EMAIL_VERIFIED", "users", otp.UserId);

            // Create default notification settings
            _notifDL.SaveSettings(new NotificationSetting { UserId = otp.UserId });

            return (true, "Email verified successfully. You can now sign in.");
        }

        public (bool Success, string Message) ResendOtp(string email)
        {
            var user = _userDL.GetUserByEmail(email);
            if (user == null) return (false, "No account found with this email.");
            if (user.IsEmailVerified) return (false, "Email is already verified.");

            var otpCode = OtpGenerator.GenerateOtp(AppConfig.OtpLength);
            var otp = new OtpVerification
            {
                UserId = user.UserId,
                OtpCode = otpCode,
                Email = email,
                ExpiresAt = DateTime.Now.AddMinutes(AppConfig.OtpExpiryMinutes),
                Purpose = "EmailVerification"
            };
            _otpDL.CreateOtp(otp);
            _ = Task.Run(async () => await EmailService.SendOtpEmailAsync(email, otpCode, user.FullName));

            return (true, "A new OTP has been sent to your email.");
        }

        public (bool Success, string Message, User? User) Login(string email, string password)
        {
            var user = _userDL.GetUserByEmail(email);
            if (user == null) return (false, "Invalid email or password.", null);
            if (!user.IsEmailVerified) return (false, "Please verify your email first.", null);
            if (user.IsBanned) return (false, $"Your account has been banned. Reason: {user.BanReason}", null);
            if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
                return (false, "Invalid email or password.", null);

            _userDL.UpdateLastLogin(user.UserId);
            _auditDL.LogAction(user.UserId, "LOGIN", "users", user.UserId);

            return (true, "Login successful.", user);
        }

        public User? GetUserById(int userId) => _userDL.GetUserById(userId);
        public List<User> GetAllUsers() => _userDL.GetAllUsers();
        public List<User> GetUsersByRole(UserRole role) => _userDL.GetUsersByRole(role);

        public bool UpdateProfile(User user)
        {
            _auditDL.LogAction(user.UserId, "PROFILE_UPDATE", "users", user.UserId);
            return _userDL.UpdateUser(user);
        }

        public (bool Success, string Message) BanUser(int userId, string reason, int adminId)
        {
            var user = _userDL.GetUserById(userId);
            if (user == null) return (false, "User not found.");
            if (user.Role == UserRole.Administrator) return (false, "Cannot ban an administrator.");

            _userDL.BanUser(userId, reason);
            _auditDL.LogAction(adminId, "BAN_USER", "users", userId, null, $"Reason: {reason}");

            _notifDL.CreateNotification(new Notification
            {
                UserId = userId,
                Title = "Account Banned",
                Message = $"Your account has been banned. Reason: {reason}",
                Type = NotificationType.AccountAlert,
                SendEmail = true
            });

            return (true, "User has been banned.");
        }

        public (bool Success, string Message) UnbanUser(int userId, int adminId)
        {
            _userDL.UnbanUser(userId);
            _auditDL.LogAction(adminId, "UNBAN_USER", "users", userId);
            return (true, "User has been unbanned.");
        }
    }
}
