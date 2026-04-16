using MySql.Data.MySqlClient;
using ECommerce.Models;
using System.Data;

namespace ECommerce.DL
{
    public class AdminRoleRequestDL
    {
        public int CreateRequest(AdminRoleRequest request)
        {
            var query = @"INSERT INTO admin_role_requests (requester_id, reason, time_limit)
                          VALUES (@rid, @reason, @limit); SELECT LAST_INSERT_ID();";
            var result = DbHelper.ExecuteScalar(query,
                new MySqlParameter("@rid", request.RequesterId),
                new MySqlParameter("@reason", request.Reason),
                new MySqlParameter("@limit", (object?)request.TimeLimit ?? DBNull.Value));
            return Convert.ToInt32(result);
        }

        public List<AdminRoleRequest> GetPendingRequests()
        {
            var query = @"SELECT r.*, u.full_name AS requester_name FROM admin_role_requests r
                          JOIN users u ON r.requester_id = u.user_id
                          WHERE r.status = 'Pending' ORDER BY r.created_at DESC";
            return MapRequests(DbHelper.ExecuteQuery(query));
        }

        public List<AdminRoleRequest> GetAllRequests()
        {
            var query = @"SELECT r.*, u.full_name AS requester_name,
                          a.full_name AS approved_by_name
                          FROM admin_role_requests r
                          JOIN users u ON r.requester_id = u.user_id
                          LEFT JOIN users a ON r.approved_by_id = a.user_id
                          ORDER BY r.created_at DESC";
            return MapRequests(DbHelper.ExecuteQuery(query));
        }

        public bool ApproveRequest(int requestId, int approvedById)
        {
            return DbHelper.ExecuteNonQuery(
                "UPDATE admin_role_requests SET status = 'Approved', approved_by_id = @aid, resolved_at = NOW() WHERE request_id = @rid",
                new MySqlParameter("@aid", approvedById), new MySqlParameter("@rid", requestId)) > 0;
        }

        public bool RejectRequest(int requestId, int rejectedById)
        {
            return DbHelper.ExecuteNonQuery(
                "UPDATE admin_role_requests SET status = 'Rejected', approved_by_id = @aid, resolved_at = NOW() WHERE request_id = @rid",
                new MySqlParameter("@aid", rejectedById), new MySqlParameter("@rid", requestId)) > 0;
        }

        public bool ExpireRequest(int requestId)
        {
            return DbHelper.ExecuteNonQuery(
                "UPDATE admin_role_requests SET status = 'Expired', resolved_at = NOW() WHERE request_id = @rid",
                new MySqlParameter("@rid", requestId)) > 0;
        }

        private List<AdminRoleRequest> MapRequests(DataTable dt)
        {
            return dt.AsEnumerable().Select(row => new AdminRoleRequest
            {
                RequestId = Convert.ToInt32(row["request_id"]),
                RequesterId = Convert.ToInt32(row["requester_id"]),
                ApprovedById = row["approved_by_id"] == DBNull.Value ? null : Convert.ToInt32(row["approved_by_id"]),
                Reason = row["reason"].ToString()!,
                Status = row["status"].ToString()!,
                TimeLimit = row["time_limit"] == DBNull.Value ? null : Convert.ToDateTime(row["time_limit"]),
                CreatedAt = Convert.ToDateTime(row["created_at"]),
                ResolvedAt = row["resolved_at"] == DBNull.Value ? null : Convert.ToDateTime(row["resolved_at"]),
                RequesterName = row.Table.Columns.Contains("requester_name") ? row["requester_name"]?.ToString() : null,
                ApprovedByName = row.Table.Columns.Contains("approved_by_name") ? row["approved_by_name"]?.ToString() : null
            }).ToList();
        }
    }
}
