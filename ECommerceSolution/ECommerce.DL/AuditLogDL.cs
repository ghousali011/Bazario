using MySql.Data.MySqlClient;
using ECommerce.Models;
using System.Data;

namespace ECommerce.DL
{
    public class AuditLogDL
    {
        public void LogAction(int? userId, string action, string tableName, int? recordId, string? oldValues = null, string? newValues = null)
        {
            var query = @"INSERT INTO audit_logs (user_id, action, table_name, record_id, old_values, new_values)
                          VALUES (@uid, @action, @table, @rid, @old, @new)";
            DbHelper.ExecuteNonQuery(query,
                new MySqlParameter("@uid", (object?)userId ?? DBNull.Value),
                new MySqlParameter("@action", action),
                new MySqlParameter("@table", tableName),
                new MySqlParameter("@rid", (object?)recordId ?? DBNull.Value),
                new MySqlParameter("@old", (object?)oldValues ?? DBNull.Value),
                new MySqlParameter("@new", (object?)newValues ?? DBNull.Value));
        }

        public List<AuditLog> GetLogs(int limit = 100)
        {
            var query = @"SELECT l.*, u.full_name AS user_name FROM audit_logs l
                          LEFT JOIN users u ON l.user_id = u.user_id
                          ORDER BY l.created_at DESC LIMIT @limit";
            var dt = DbHelper.ExecuteQuery(query, new MySqlParameter("@limit", limit));
            return dt.AsEnumerable().Select(row => new AuditLog
            {
                LogId = Convert.ToInt64(row["log_id"]),
                UserId = row["user_id"] == DBNull.Value ? null : Convert.ToInt32(row["user_id"]),
                Action = row["action"].ToString()!,
                TableName = row["table_name"].ToString()!,
                RecordId = row["record_id"] == DBNull.Value ? null : Convert.ToInt32(row["record_id"]),
                OldValues = row["old_values"]?.ToString(),
                NewValues = row["new_values"]?.ToString(),
                CreatedAt = Convert.ToDateTime(row["created_at"]),
                UserName = row.Table.Columns.Contains("user_name") ? row["user_name"]?.ToString() : null
            }).ToList();
        }

        public List<AuditLog> GetLogsByUser(int userId)
        {
            var query = @"SELECT l.*, u.full_name AS user_name FROM audit_logs l
                          LEFT JOIN users u ON l.user_id = u.user_id
                          WHERE l.user_id = @uid ORDER BY l.created_at DESC LIMIT 100";
            var dt = DbHelper.ExecuteQuery(query, new MySqlParameter("@uid", userId));
            return dt.AsEnumerable().Select(row => new AuditLog
            {
                LogId = Convert.ToInt64(row["log_id"]),
                UserId = row["user_id"] == DBNull.Value ? null : Convert.ToInt32(row["user_id"]),
                Action = row["action"].ToString()!,
                TableName = row["table_name"].ToString()!,
                RecordId = row["record_id"] == DBNull.Value ? null : Convert.ToInt32(row["record_id"]),
                CreatedAt = Convert.ToDateTime(row["created_at"]),
                UserName = row.Table.Columns.Contains("user_name") ? row["user_name"]?.ToString() : null
            }).ToList();
        }
    }
}
