using ECommerce.BL;
using ECommerce.Models;
using ECommerce.Models.Enums;

namespace ECommerce.UI.Forms
{
    public partial class SellerDashboardForm : Form
    {
        private readonly ProductBL _productBL = new();
        private readonly OrderBL _orderBL = new();
        private readonly NotificationBL _notifBL = new();

        private TabControl tabControl = null!;
        private DataGridView dgvProducts = null!;
        private DataGridView dgvOrders = null!;
        private DataGridView dgvNotifications = null!;
        private Label lblUnread = null!;

        public SellerDashboardForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = $"Seller Dashboard - {SessionManager.CurrentUser?.FullName}";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(236, 240, 241);

            var lblWelcome = new Label { Text = $"Seller: {SessionManager.CurrentUser?.FullName}", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80), Location = new Point(15, 10), AutoSize = true };
            lblUnread = new Label { Location = new Point(700, 15), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.Red };
            var btnLogout = new Button { Text = "Logout", Location = new Point(880, 10), Size = new Size(80, 40), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLogout.Click += (s, e) => this.Close();

            tabControl = new TabControl { Location = new Point(10, 45), Size = new Size(965, 555), Font = new Font("Segoe UI", 10) };

            // My Products Tab
            var tabProducts = new TabPage("My Products");
            var btnAdd = new Button { Text = "Add Product", Location = new Point(10, 10), Size = new Size(120, 40), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.Click += BtnAddProduct_Click;
            var btnEdit = new Button { Text = "Edit Product", Location = new Point(140, 10), Size = new Size(120, 40), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnEdit.Click += BtnEditProduct_Click;
            var btnDelete = new Button { Text = "Delete Product", Location = new Point(270, 10), Size = new Size(120, 40), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete.Click += BtnDeleteProduct_Click;
            dgvProducts = CreateGrid(new Point(10, 50), new Size(940, 440));
            tabProducts.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, dgvProducts });

            // Orders Tab
            var tabOrders = new TabPage("Orders");
            dgvOrders = CreateGrid(new Point(10, 10), new Size(940, 430));
            var btnUpdateStatus = new Button { Text = "Update Status", Location = new Point(10, 450), Size = new Size(140, 70), BackColor = Color.FromArgb(243, 156, 18), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpdateStatus.Click += BtnUpdateStatus_Click;
            tabOrders.Controls.AddRange(new Control[] { dgvOrders, btnUpdateStatus });

            // Notifications Tab
            var tabNotif = new TabPage("Notifications");
            dgvNotifications = CreateGrid(new Point(10, 10), new Size(940, 450));
            var btnDeleteNotification = new Button { Text = "Delete Notification", Location = new Point(10, 450), Size = new Size(140, 70), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDeleteNotification.Click += BtnDeleteNotification_Click;
            _notifBL.MarkAllAsRead(SessionManager.CurrentUser!.UserId);
            tabNotif.Controls.AddRange(new Control[] { dgvNotifications, btnDeleteNotification });

            tabControl.TabPages.AddRange(new[] { tabProducts, tabOrders, tabNotif });
            tabControl.SelectedIndexChanged += (s, e) => LoadData();
            dgvOrders.CellDoubleClick += DgvOrders_CellDoubleClick;

            this.Controls.AddRange(new Control[] { lblWelcome, lblUnread, btnLogout, tabControl });
        }

        private void DgvOrders_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvOrders.Rows[e.RowIndex];
            if (row == null) return;
            if (!dgvOrders.Columns.Contains("OrderId")) return;
            var orderIdObj = row.Cells["OrderId"].Value;
            if (orderIdObj == null) return;
            if (!int.TryParse(orderIdObj.ToString(), out var orderId)) return;

            ShowOrderDetails(orderId);
        }

        private void ShowOrderDetails(int orderId)
        {
            var order = _orderBL.GetOrderById(orderId);
            if (order == null) { MessageBox.Show("Order not found."); return; }

            using var details = new OrderDetailsForm(order);
            details.ShowDialog(this);
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
            dgvProducts.DataSource = _productBL.GetProductsBySeller(uid)
                .Select(p => new { p.ProductId, p.ProductName, p.CategoryName, Price = $"${p.Price:F2}", p.StockQuantity, Rating = $"{p.AverageRating:F1}", Banned = p.IsBanned ? "Yes" : "No" }).ToList();

            dgvOrders.DataSource = _orderBL.GetOrdersBySeller(uid)
                .Select(o => new {o.OrderId, Customer = o.CustomerName, Date = o.OrderDate.ToString("g"), Total = $"${o.TotalAmount:F2}", Status = o.Status.ToString() }).ToList();

            var notifs = _notifBL.GetNotifications(uid);
            dgvNotifications.DataSource = notifs.Select(n => new { n.NotificationId, n.Title, n.Message, Date = n.CreatedAt.ToString("g"), Read = n.IsRead ? "Yes" : "No" }).ToList();
            lblUnread.Text = _notifBL.GetUnreadCount(uid) is var c and > 0 ? $"🔔 {c} unread" : "";
        }

        private void BtnAddProduct_Click(object? sender, EventArgs e)
        {
            var form = new ProductForm();
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnEditProduct_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0) { MessageBox.Show("Select a product."); return; }
            var pid = (int)dgvProducts.SelectedRows[0].Cells["ProductId"].Value;
            var product = _productBL.GetProductById(pid);
            if (product == null) return;
            var form = new ProductForm(product);
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnDeleteProduct_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0) return;
            if (MessageBox.Show("Delete this product?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            var pid = (int)dgvProducts.SelectedRows[0].Cells["ProductId"].Value;
            _productBL.DeleteProduct(pid, SessionManager.CurrentUser!.UserId);
            LoadData();
        }

        private void BtnUpdateStatus_Click(object? sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0) { MessageBox.Show("Select an order."); return; }
            var orderId = (int)dgvOrders.SelectedRows[0].Cells["OrderId"].Value;

            var statusForm = new Form { Text = "Update Order Status", Size = new Size(300, 180), StartPosition = FormStartPosition.CenterParent, BackColor = Color.White };
            var cmb = new ComboBox { Location = new Point(30, 30), Size = new Size(220, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var s in Enum.GetValues<OrderStatus>()) cmb.Items.Add(s);
            cmb.SelectedIndex = 0;
            var btn = new Button { Text = "Update", Location = new Point(30, 70), Size = new Size(220, 35), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btn.Click += (s2, e2) =>
            {
                var status = (OrderStatus)cmb.SelectedItem!;
                var (success, msg) = _orderBL.UpdateOrderStatus(orderId, status, SessionManager.CurrentUser!.UserId);
                MessageBox.Show(msg);
                if (success) { statusForm.Close(); LoadData(); }
            };
            statusForm.Controls.AddRange(new Control[] { cmb, btn });
            statusForm.ShowDialog();
        }

        private void BtnDeleteNotification_Click(object? sender, EventArgs e)
        {
            if (dgvNotifications.SelectedRows.Count == 0) return;
            var nid = (int)dgvNotifications.SelectedRows[0].Cells["NotificationId"].Value;
            _notifBL.DeleteNotification(nid);
            LoadData();
        }
    }
}
