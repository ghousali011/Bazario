using ECommerce.BL;
using ECommerce.Models;
using ECommerce.Models.Enums;

namespace ECommerce.UI.Forms
{
    public partial class AdminDashboardForm : Form
    {
        private readonly UserBL _userBL = new();
        private readonly ProductBL _productBL = new();
        private readonly OrderBL _orderBL = new();
        private readonly AdminBL _adminBL = new();
        private readonly NotificationBL _notifBL = new();

        private TabControl tabControl = null!;
        private DataGridView dgvUsers = null!;
        private DataGridView dgvProducts = null!;
        private DataGridView dgvOrders = null!;
        private DataGridView dgvRequests = null!;
        private DataGridView dgvLogs = null!;
        private DataGridView dgvNotifications = null!;

        public AdminDashboardForm()
        {
            InitializeComponent();
            LoadData();

            // Check expired admin access on load
            _adminBL.CheckExpiredAdminAccess();
        }

        private void InitializeComponent()
        {
            this.Text = $"Admin Dashboard - {SessionManager.CurrentUser?.FullName}";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(236, 240, 241);

            var lblWelcome = new Label { Text = $"Administrator: {SessionManager.CurrentUser?.FullName}", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80), Location = new Point(15, 10), AutoSize = true };
            var btnLogout = new Button { Text = "Logout", Location = new Point(980, 10), Size = new Size(80, 40), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLogout.Click += (s, e) => this.Close();

            tabControl = new TabControl { Location = new Point(10, 45), Size = new Size(1065, 605), Font = new Font("Segoe UI", 10) };

            // Users Tab
            var tabUsers = new TabPage("Manage Users");
            dgvUsers = CreateGrid(new Point(10, 10), new Size(1040, 480));
            var btnBan = new Button { Text = "Ban User", Location = new Point(10, 500), Size = new Size(120, 40), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnBan.Click += BtnBanUser_Click;
            var btnUnban = new Button { Text = "Unban User", Location = new Point(140, 500), Size = new Size(120, 40), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUnban.Click += BtnUnbanUser_Click;
            tabUsers.Controls.AddRange(new Control[] { dgvUsers, btnBan, btnUnban });

            // Products Tab
            var tabProducts = new TabPage("Manage Products");
            dgvProducts = CreateGrid(new Point(10, 10), new Size(1040, 480));
            var btnBanProd = new Button { Text = "Ban Product", Location = new Point(10, 500), Size = new Size(120, 30), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnBanProd.Click += BtnBanProduct_Click;
            var btnUnbanProd = new Button { Text = "Unban Product", Location = new Point(140, 500), Size = new Size(120, 30), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUnbanProd.Click += BtnUnbanProduct_Click;
            tabProducts.Controls.AddRange(new Control[] { dgvProducts, btnBanProd, btnUnbanProd });

            // Orders Tab
            var tabOrders = new TabPage("All Orders");
            dgvOrders = CreateGrid(new Point(10, 10), new Size(1040, 510));
            tabOrders.Controls.Add(dgvOrders);

            // Admin Requests Tab
            var tabRequests = new TabPage("Admin Requests");
            dgvRequests = CreateGrid(new Point(10, 10), new Size(1040, 470));
            var btnApprove = new Button { Text = "Approve", Location = new Point(10, 490), Size = new Size(120, 30), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnApprove.Click += BtnApproveRequest_Click;
            var btnReject = new Button { Text = "Reject", Location = new Point(140, 490), Size = new Size(120, 30), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnReject.Click += BtnRejectRequest_Click;
            tabRequests.Controls.AddRange(new Control[] { dgvRequests, btnApprove, btnReject });

            // Audit Logs Tab
            var tabLogs = new TabPage("Audit Logs");
            dgvLogs = CreateGrid(new Point(10, 10), new Size(1040, 510));
            tabLogs.Controls.Add(dgvLogs);

            // Notifications Tab
            var tabNotif = new TabPage("Notifications");
            dgvNotifications = CreateGrid(new Point(10, 10), new Size(1040, 510));
            tabNotif.Controls.Add(dgvNotifications);

            // Categories Tab
            var tabCat = new TabPage("Categories");
            var btnAddCat = new Button { Text = "Add Category", Location = new Point(10, 10), Size = new Size(130, 30), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddCat.Click += BtnAddCategory_Click;
            var dgvCat = CreateGrid(new Point(10, 50), new Size(1040, 470));
            dgvCat.Tag = "categories";
            tabCat.Controls.AddRange(new Control[] { btnAddCat, dgvCat });

            tabControl.TabPages.AddRange(new[] { tabUsers, tabProducts, tabOrders, tabRequests, tabLogs, tabNotif, tabCat });
            tabControl.SelectedIndexChanged += (s, e) => LoadData();

            this.Controls.AddRange(new Control[] { lblWelcome, btnLogout, tabControl });
        }

        private DataGridView CreateGrid(Point location, Size size) => new DataGridView
        {
            Location = location, Size = size, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true, AllowUserToAddRows = false,
            BackgroundColor = Color.White, BorderStyle = BorderStyle.None, RowHeadersVisible = false
        };

        private void LoadData()
        {
            var uid = SessionManager.CurrentUser!.UserId;

            dgvUsers.DataSource = _userBL.GetAllUsers()
                .Select(u => new { u.UserId, u.FullName, u.Email, Role = u.Role.ToString(), Banned = u.IsBanned ? "Yes" : "No", Verified = u.IsEmailVerified ? "Yes" : "No", Joined = u.CreatedAt.ToString("d") }).ToList();

            dgvProducts.DataSource = _productBL.GetAllActiveProducts()
                .Select(p => new { p.ProductId, p.ProductName, Seller = p.SellerName, Price = $"${p.Price:F2}", p.StockQuantity, Banned = p.IsBanned ? "Yes" : "No" }).ToList();

            dgvOrders.DataSource = _orderBL.GetAllOrders()
                .Select(o => new { o.OrderId, Customer = o.CustomerName, Date = o.OrderDate.ToString("g"), Total = $"${o.TotalAmount:F2}", Status = o.Status.ToString() }).ToList();

            dgvRequests.DataSource = _adminBL.GetAllRequests()
                .Select(r => new { r.RequestId, Requester = r.RequesterName, r.Reason, r.Status, TimeLimit = r.TimeLimit?.ToString("g") ?? "Permanent", Date = r.CreatedAt.ToString("g") }).ToList();

            dgvLogs.DataSource = _adminBL.GetAuditLogs(200)
                .Select(l => new { l.LogId, User = l.UserName ?? "System", l.Action, l.TableName, l.RecordId, Date = l.CreatedAt.ToString("g") }).ToList();

            dgvNotifications.DataSource = _notifBL.GetNotifications(uid)
                .Select(n => new { n.Title, n.Message, Date = n.CreatedAt.ToString("g") }).ToList();

            // Load categories in the categories tab grid
            foreach (TabPage tab in tabControl.TabPages)
            {
                foreach (Control c in tab.Controls)
                {
                    if (c is DataGridView dg && dg.Tag?.ToString() == "categories")
                    {
                        dg.DataSource = _productBL.GetAllCategories()
                            .Select(cat => new { cat.CategoryId, cat.CategoryName, cat.Description }).ToList();
                    }
                }
            }
        }

        private void BtnBanUser_Click(object? sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0) return;
            var userId = (int)dgvUsers.SelectedRows[0].Cells["UserId"].Value;
            var reason = Microsoft.VisualBasic.Interaction.InputBox("Enter ban reason:", "Ban User", "");
            if (string.IsNullOrWhiteSpace(reason)) return;
            var (success, msg) = _userBL.BanUser(userId, reason, SessionManager.CurrentUser!.UserId);
            MessageBox.Show(msg);
            if (success) LoadData();
        }

        private void BtnUnbanUser_Click(object? sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0) return;
            var userId = (int)dgvUsers.SelectedRows[0].Cells["UserId"].Value;
            _userBL.UnbanUser(userId, SessionManager.CurrentUser!.UserId);
            LoadData();
        }

        private void BtnBanProduct_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0) return;
            var productId = (int)dgvProducts.SelectedRows[0].Cells["ProductId"].Value;
            var reason = Microsoft.VisualBasic.Interaction.InputBox("Enter ban reason:", "Ban Product", "");
            if (string.IsNullOrWhiteSpace(reason)) return;
            var (success, msg) = _productBL.BanProduct(productId, reason, SessionManager.CurrentUser!.UserId);
            MessageBox.Show(msg);
            if (success) LoadData();
        }

        private void BtnUnbanProduct_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0) return;
            var productId = (int)dgvProducts.SelectedRows[0].Cells["ProductId"].Value;
            _productBL.UnbanProduct(productId, SessionManager.CurrentUser!.UserId);
            LoadData();
        }

        private void BtnApproveRequest_Click(object? sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count == 0) return;
            var requestId = (int)dgvRequests.SelectedRows[0].Cells["RequestId"].Value;
            _adminBL.ApproveRequest(requestId, SessionManager.CurrentUser!.UserId);
            MessageBox.Show("Request approved.");
            LoadData();
        }

        private void BtnRejectRequest_Click(object? sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count == 0) return;
            var requestId = (int)dgvRequests.SelectedRows[0].Cells["RequestId"].Value;
            _adminBL.RejectRequest(requestId, SessionManager.CurrentUser!.UserId);
            MessageBox.Show("Request rejected.");
            LoadData();
        }

        private void BtnAddCategory_Click(object? sender, EventArgs e)
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox("Category Name:", "Add Category", "");
            if (string.IsNullOrWhiteSpace(name)) return;
            var desc = Microsoft.VisualBasic.Interaction.InputBox("Description (optional):", "Add Category", "");
            _productBL.AddCategory(new Category { CategoryName = name, Description = string.IsNullOrWhiteSpace(desc) ? null : desc });
            MessageBox.Show("Category added.");
            LoadData();
        }
    }
}
