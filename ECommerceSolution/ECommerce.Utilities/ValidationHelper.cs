using System.Text.RegularExpressions;

namespace ECommerce.Utilities
{
    public static class ValidationHelper
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8) return false;
            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));
            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            return Regex.IsMatch(phone, @"^\+?[\d\s\-()]{7,15}$");
        }

        public static (bool IsValid, string Message) ValidateRegistration(string name, string email, string password, string confirmPassword, string phone)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Full name is required.");
            if (!IsValidEmail(email))
                return (false, "Please enter a valid email address.");
            if (!IsValidPassword(password))
                return (false, "Password must be at least 8 characters with uppercase, lowercase, digit, and special character.");
            if (password != confirmPassword)
                return (false, "Passwords do not match.");
            if (!string.IsNullOrWhiteSpace(phone) && !IsValidPhone(phone))
                return (false, "Please enter a valid phone number.");
            return (true, "Valid");
        }
    }
}
