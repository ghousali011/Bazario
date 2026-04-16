using System.Security.Cryptography;

namespace ECommerce.Utilities
{
    public static class OtpGenerator
    {
        public static string GenerateOtp(int length = 6)
        {
            var otp = new char[length];
            for (int i = 0; i < length; i++)
            {
                otp[i] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
            }
            return new string(otp);
        }
    }
}
