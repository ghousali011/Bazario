using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace ECommerce.Utilities
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            const int saltSize = 16;
            const int iterations = 10000;
            const int hashSize = 32;

            var salt = RandomNumberGenerator.GetBytes(saltSize);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(hashSize);

            var saltB64 = Convert.ToBase64String(salt);
            var hashB64 = Convert.ToBase64String(hash);

            return $"pbkdf2_sha256${iterations}${saltB64}${hashB64}";
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash)) return false;

            storedHash = storedHash.Trim();

            if (storedHash.StartsWith("pbkdf2_sha256$", StringComparison.Ordinal))
            {
                var parts = storedHash.Split('$');
                if (parts.Length != 4) return false;

                if (!int.TryParse(parts[1], out var iterations)) return false;
                var saltBytes = Convert.FromBase64String(parts[2]);
                var storedHashBytes = Convert.FromBase64String(parts[3]);

                using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
                var computedHash = pbkdf2.GetBytes(storedHashBytes.Length);
                return CryptographicOperations.FixedTimeEquals(storedHashBytes, computedHash);
            }

            var legacyParts = storedHash.Split(':');
            if (legacyParts.Length != 2) return false;

            try
            {
                var saltBytesLegacy = Convert.FromBase64String(legacyParts[0]);
                var storedHashBytesLegacy = Convert.FromBase64String(legacyParts[1]);

                using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytesLegacy, 10000, HashAlgorithmName.SHA256);
                var computedHashLegacy = pbkdf2.GetBytes(storedHashBytesLegacy.Length);
                return CryptographicOperations.FixedTimeEquals(storedHashBytesLegacy, computedHashLegacy);
            }
            catch
            {
                return false;
            }
        }
    }
}
