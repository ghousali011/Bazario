using MySql.Data.MySqlClient;
using ECommerce.Models;
using System.Data;

namespace ECommerce.DL
{
    public class OtpDL
    {
        public int CreateOtp(OtpVerification otp)
        {
            // Invalidate previous OTPs for this email/purpose
            DbHelper.ExecuteNonQuery(
                "UPDATE otp_verifications SET is_used = 1 WHERE email = @email AND purpose = @purpose AND is_used = 0",
                new MySqlParameter("@email", otp.Email),
                new MySqlParameter("@purpose", otp.Purpose));

            var query = @"INSERT INTO otp_verifications (user_id, otp_code, email, expires_at, purpose)
                          VALUES (@uid, @code, @email, @exp, @purpose); SELECT LAST_INSERT_ID();";
            var result = DbHelper.ExecuteScalar(query,
                new MySqlParameter("@uid", otp.UserId),
                new MySqlParameter("@code", otp.OtpCode),
                new MySqlParameter("@email", otp.Email),
                new MySqlParameter("@exp", otp.ExpiresAt),
                new MySqlParameter("@purpose", otp.Purpose));
            return Convert.ToInt32(result);
        }

        public OtpVerification? GetValidOtp(string email, string code, string purpose)
        {
            var query = @"SELECT * FROM otp_verifications
                          WHERE email = @email AND otp_code = @code AND purpose = @purpose
                          AND is_used = 0 AND expires_at > NOW()
                          ORDER BY created_at DESC LIMIT 1";
            var dt = DbHelper.ExecuteQuery(query,
                new MySqlParameter("@email", email),
                new MySqlParameter("@code", code),
                new MySqlParameter("@purpose", purpose));
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new OtpVerification
            {
                OtpId = Convert.ToInt32(row["otp_id"]),
                UserId = Convert.ToInt32(row["user_id"]),
                OtpCode = row["otp_code"].ToString()!,
                Email = row["email"].ToString()!,
                ExpiresAt = Convert.ToDateTime(row["expires_at"]),
                IsUsed = Convert.ToBoolean(row["is_used"]),
                Purpose = row["purpose"].ToString()!
            };
        }

        public bool MarkOtpUsed(int otpId)
        {
            return DbHelper.ExecuteNonQuery("UPDATE otp_verifications SET is_used = 1 WHERE otp_id = @id",
                new MySqlParameter("@id", otpId)) > 0;
        }
    }
}
