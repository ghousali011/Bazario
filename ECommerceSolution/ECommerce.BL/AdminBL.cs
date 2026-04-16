using ECommerce.DL;
using ECommerce.Models;
using ECommerce.Models.Enums;

namespace ECommerce.BL
{
    public class AdminBL
    {
        private readonly AdminRoleRequestDL _requestDL = new();
        private readonly UserDL _userDL = new();
        private readonly AuditLogDL _auditDL = new();
        private readonly NotificationDL _notifDL = new();

        public (bool Success, string Message) RequestAdminRole(int requesterId, string reason, DateTime? timeLimit)
        {
            var user = _userDL.GetUserById(requesterId);
            if (user == null) return (false, "User not found.");
            if (user.Role == UserRole.Administrator)
                return (false, "You are already an administrator.");

            _requestDL.CreateRequest(new AdminRoleRequest
            {
                RequesterId = requesterId,
                Reason = reason,
                TimeLimit = timeLimit
            });

            _auditDL.LogAction(requesterId, "REQUEST_ADMIN_ROLE", "admin_role_requests", requesterId);

            // Notify existing admins
            var admins = _userDL.GetUsersByRole(UserRole.Administrator);
            foreach (var admin in admins)
            {
                _notifDL.CreateNotification(new Notification
                {
                    UserId = admin.UserId,
                    Title = "Admin Role Request",
                    Message = $"{user.FullName} has requested administrator access. Reason: {reason}",
                    Type = NotificationType.AdminAlert,
                    SendEmail = true
                });
            }

            return (true, "Admin role request submitted. Waiting for approval.");
        }

        public (bool Success, string Message) ApproveRequest(int requestId, int approvedById)
        {
            _requestDL.ApproveRequest(requestId, approvedById);

            var requests = _requestDL.GetAllRequests();
            var request = requests.FirstOrDefault(r => r.RequestId == requestId);
            if (request != null)
            {
                _userDL.UpdateRole(request.RequesterId, UserRole.Administrator);
                _auditDL.LogAction(approvedById, "APPROVE_ADMIN_REQUEST", "admin_role_requests", requestId);

                _notifDL.CreateNotification(new Notification
                {
                    UserId = request.RequesterId,
                    Title = "Admin Access Granted",
                    Message = request.TimeLimit.HasValue
                        ? $"Your administrator access has been approved until {request.TimeLimit:g}."
                        : "Your administrator access has been approved.",
                    Type = NotificationType.AdminAlert,
                    SendEmail = true
                });
            }

            return (true, "Request approved. User granted administrator access.");
        }

        public (bool Success, string Message) RejectRequest(int requestId, int rejectedById)
        {
            _requestDL.RejectRequest(requestId, rejectedById);
            _auditDL.LogAction(rejectedById, "REJECT_ADMIN_REQUEST", "admin_role_requests", requestId);
            return (true, "Request rejected.");
        }

        public List<AdminRoleRequest> GetPendingRequests() => _requestDL.GetPendingRequests();
        public List<AdminRoleRequest> GetAllRequests() => _requestDL.GetAllRequests();

        public List<AuditLog> GetAuditLogs(int limit = 100) => new AuditLogDL().GetLogs(limit);
        public List<AuditLog> GetUserAuditLogs(int userId) => new AuditLogDL().GetLogsByUser(userId);

        public void CheckExpiredAdminAccess()
        {
            var requests = _requestDL.GetAllRequests()
                .Where(r => r.Status == "Approved" && r.TimeLimit.HasValue && r.TimeLimit < DateTime.Now);

            foreach (var req in requests)
            {
                // Revert role
                var user = _userDL.GetUserById(req.RequesterId);
                if (user != null && user.Role == UserRole.Administrator)
                {
                    // Check if this is the original admin (user_id = 1 convention)
                    if (user.UserId != 1) // Don't demote the original admin
                    {
                        _userDL.UpdateRole(req.RequesterId, UserRole.Customer);
                        _requestDL.ExpireRequest(req.RequestId);
                        _auditDL.LogAction(null, "EXPIRE_ADMIN_ACCESS", "admin_role_requests", req.RequestId);

                        _notifDL.CreateNotification(new Notification
                        {
                            UserId = req.RequesterId,
                            Title = "Admin Access Expired",
                            Message = "Your temporary administrator access has expired.",
                            Type = NotificationType.AdminAlert,
                            SendEmail = true
                        });
                    }
                }
            }
        }
    }
}
