using ECommerce.BL;
using ECommerce.Models;
using ECommerce.Models.Enums;

namespace ECommerce.UI.Forms
{
    public partial class CustomerDashboardForm : Form
    {
        private readonly ProductBL _productBL = new();
        private readonly CartBL _cartBL = new();
        private readonly OrderBL _orderBL = new();
        private readonly ReviewBL _reviewBL = new();
        private readonly NotificationBL _notifBL = new();

        private TabControl tabControl = null!;
        private DataGridView dgvProducts = null!;
        private DataGridView dgvCart = null!;
        private DataGridView dgvOrders = null!;
        private DataGridView dgvNotifications = null!;
        private TextBox txtSearch = null!;
        private Button btnSearch = null!;
        private Button btnAddToCart = null!;
        private Button btnPlaceOrder = null!;
        private Button btnRemoveFromCart = null!;
        private Label lblCartTotal = null!;
        private Label lblWelcome = null!;
        private Label lblUnread = null!;

        public CustomerDashboardForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = $"Customer Dashboard - {SessionManager.CurrentUser?.FullName}";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(236, 240, 241);

            lblWelcome = new Label { Text = $"Welcome, {SessionManager.CurrentUser?.FullName}!", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80), Location = new Point(15, 10), AutoSize = true };
            lblUnread = new Label { Location = new Point(750, 15), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.Red };

            var btnLogout = new Button { Text = "Logout", Location = new Point(880, 10), Size = new Size(80, 40), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => this.Close();

            tabControl = new TabControl { Location = new Point(10, 45), Size = new Size(965, 555), Font = new Font("Segoe UI", 10) };

            // Products Tab
            var tabProducts = new TabPage("Browse Products");
            txtSearch = new TextBox { Location = new Point(10, 10), Size = new Size(300, 28), Font = new Font("Segoe UI", 10), PlaceholderText = "Search products..." };
            btnSearch = new Button { Text = "Search", Location = new Point(320, 10), Size = new Size(80, 35), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSearch.Click += (s, e) => SearchProducts();
            btnAddToCart = new Button { Text = "Add to Cart", Location = new Point(720, 10), Size = new Size(220, 35), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddToCart.Click += BtnAddToCart_Click;
            dgvProducts = CreateGrid(new Point(10, 45), new Size(940, 450));
            tabProducts.Controls.AddRange(new Control[] { txtSearch, btnSearch, btnAddToCart, dgvProducts });

            // Cart Tab
            var tabCart = new TabPage("My Cart");
            dgvCart = CreateGrid(new Point(10, 10), new Size(940, 380));
            lblCartTotal = new Label { Text = "Total: $0.00", Location = new Point(10, 400), Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80), AutoSize = true };
            btnRemoveFromCart = new Button { Text = "Remove Selected", Location = new Point(600, 400), Size = new Size(140, 40), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRemoveFromCart.Click += BtnRemoveFromCart_Click;
            btnPlaceOrder = new Button { Text = "Place Order", Location = new Point(750, 400), Size = new Size(140, 40), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnPlaceOrder.Click += BtnPlaceOrder_Click;
            tabCart.Controls.AddRange(new Control[] { dgvCart, lblCartTotal, btnRemoveFromCart, btnPlaceOrder });

            // Orders Tab
            var tabOrders = new TabPage("My Orders");
            dgvOrders = CreateGrid(new Point(10, 10), new Size(940, 440));
            var btnReview = new Button { Text = "Write Review", Location = new Point(730, 460), Size = new Size(200, 40), BackColor = Color.FromArgb(155, 89, 182), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnReview.Click += BtnReview_Click;
            tabOrders.Controls.AddRange(new Control[] { dgvOrders, btnReview });

            // Notifications Tab
            var tabNotif = new TabPage("Notifications");
            dgvNotifications = CreateGrid(new Point(10, 10), new Size(940, 430));
            var btnMarkRead = new Button { Text = "Mark All Read", Location = new Point(720, 450), Size = new Size(200, 40), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnMarkRead.Click += (s, e) => { _notifBL.MarkAllAsRead(SessionManager.CurrentUser!.UserId); LoadNotifications(); };
            var btndeleteNotification = new Button { Text = "Delete Notification", Location = new Point(20, 450), Size = new Size(200, 40), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btndeleteNotification.Click += BtnDeleteNotification_Click;
            tabNotif.Controls.AddRange(new Control[] { dgvNotifications, btnMarkRead, btndeleteNotification });

            tabControl.TabPages.AddRange(new[] { tabProducts, tabCart, tabOrders, tabNotif });
            tabControl.SelectedIndexChanged += (s, e) => LoadData();

            this.Controls.AddRange(new Control[] { lblWelcome, lblUnread, btnLogout, tabControl });
        }

        private DataGridView CreateGrid(Point location, Size size)
        {
            return new DataGridView
            {
                Location = location, Size = size, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true,
                AllowUserToAddRows = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                RowHeadersVisible = false, Font = new Font("Segoe UI", 9)
            };
        }

        private void LoadData()
        {
            var uid = SessionManager.CurrentUser!.UserId;
            var products = _productBL.GetAllActiveProducts();
            dgvProducts.DataSource = products.Select(p => new { p.ProductId, p.ProductName, p.CategoryName, Price = $"${p.Price:F2}", Discount = p.DiscountPrice.HasValue ? $"${p.DiscountPrice:F2}" : "-", p.StockQuantity, Rating = $"{p.AverageRating:F1} ({p.TotalReviews})", Seller = p.SellerName }).ToList();

            var cart = _cartBL.GetCartItems(uid);
            dgvCart.DataSource = cart.Select(c => new { c.CartItemId, c.ProductName, Price = $"${c.Price:F2}", c.Quantity, Total = $"${c.TotalPrice:F2}" }).ToList();
            lblCartTotal.Text = $"Total: ${cart.Sum(c => c.TotalPrice):F2}";

            var orders = _orderBL.GetOrdersByCustomer(uid);
            dgvOrders.DataSource = orders.Select(o => new { o.OrderId, Date = o.OrderDate.ToString("g"), Total = $"${o.TotalAmount:F2}", Status = o.Status.ToString(), o.ShippingAddress }).ToList();

            LoadNotifications();
        }

        private void LoadNotifications()
        {
            var uid = SessionManager.CurrentUser!.UserId;
            var notifs = _notifBL.GetNotifications(uid);
            dgvNotifications.DataSource = notifs.Select(n => new { n.NotificationId, n.Title, n.Message, n.Type, Read = n.IsRead ? "Yes" : "No", Date = n.CreatedAt.ToString("g") }).ToList();
            var unread = _notifBL.GetUnreadCount(uid);
            lblUnread.Text = unread > 0 ? $"🔔 {unread} unread notifications" : "";
        }

        private void SearchProducts()
        {
            var keyword = txtSearch.Text.Trim();
            var products = string.IsNullOrEmpty(keyword) ? _productBL.GetAllActiveProducts() : _productBL.SearchProducts(keyword);
            dgvProducts.DataSource = products.Select(p => new { p.ProductId, p.ProductName, p.CategoryName, Price = $"${p.Price:F2}", p.StockQuantity, Rating = $"{p.AverageRating:F1}", Seller = p.SellerName }).ToList();
        }

        private void BtnAddToCart_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0) { MessageBox.Show("Select a product first."); return; }
            var productId = (int)dgvProducts.SelectedRows[0].Cells["ProductId"].Value;
            var (success, msg) = _cartBL.AddToCart(SessionManager.CurrentUser!.UserId, productId);
            MessageBox.Show(msg);
            if (success) LoadData();
        }

        private void BtnRemoveFromCart_Click(object? sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count == 0) { MessageBox.Show("Select an item first."); return; }
            var cartItemId = (int)dgvCart.SelectedRows[0].Cells["CartItemId"].Value;
            _cartBL.RemoveFromCart(cartItemId);
            LoadData();
        }

        private void BtnPlaceOrder_Click(object? sender, EventArgs e)
        {
            var address = Microsoft.VisualBasic.Interaction.InputBox("Enter shipping address:", "Shipping Address", "");
            if (string.IsNullOrWhiteSpace(address)) return;
            var (success, msg, _) = _orderBL.PlaceOrder(SessionManager.CurrentUser!.UserId, address, "COD");
            MessageBox.Show(msg, success ? "Success" : "Error", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (success) LoadData();
        }

        private void BtnReview_Click(object? sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0) { MessageBox.Show("Select an order first."); return; }
            var orderId = (int)dgvOrders.SelectedRows[0].Cells["OrderId"].Value;
            var order = _orderBL.GetOrderById(orderId);
            if (order == null || order.Status != OrderStatus.Delivered) { MessageBox.Show("You can only review delivered orders."); return; }

            var reviewForm = new ReviewForm(orderId, order.Items);
            reviewForm.ShowDialog();
            LoadData();
        }

        private void BtnDeleteNotification_Click(object? sender, EventArgs e)
        {
            if (dgvNotifications.SelectedRows.Count == 0) { MessageBox.Show("Select a notification first."); return; }
            var notificationId = (int)dgvNotifications.SelectedRows[0].Cells["NotificationId"].Value;
            _notifBL.DeleteNotification(notificationId);
            LoadNotifications();
        }
    }
}
