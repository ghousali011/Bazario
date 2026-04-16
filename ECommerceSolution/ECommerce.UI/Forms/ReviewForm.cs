using ECommerce.BL;
using ECommerce.Models;

namespace ECommerce.UI.Forms
{
    public partial class ReviewForm : Form
    {
        private readonly ReviewBL _reviewBL = new();
        private readonly int _orderId;
        private readonly List<OrderItem> _items;

        private ComboBox cmbProduct = null!;
        private NumericUpDown nudRating = null!;
        private TextBox txtComment = null!;
        private Button btnSubmit = null!;

        public ReviewForm(int orderId, List<OrderItem> items)
        {
            _orderId = orderId;
            _items = items;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Write Review";
            this.Size = new Size(420, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            var lblProduct = new Label { Text = "Product:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10) };
            cmbProduct = new ComboBox { Location = new Point(20, 45), Size = new Size(360, 28), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            foreach (var item in _items) cmbProduct.Items.Add($"{item.ProductName} (ID: {item.ProductId})");
            if (cmbProduct.Items.Count > 0) cmbProduct.SelectedIndex = 0;

            var lblRating = new Label { Text = "Rating (1-5):", Location = new Point(20, 85), AutoSize = true, Font = new Font("Segoe UI", 10) };
            nudRating = new NumericUpDown { Location = new Point(20, 110), Size = new Size(80, 28), Minimum = 1, Maximum = 5, Value = 5, Font = new Font("Segoe UI", 10) };

            var lblComment = new Label { Text = "Comment:", Location = new Point(20, 150), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtComment = new TextBox { Location = new Point(20, 175), Size = new Size(360, 80), Multiline = true, Font = new Font("Segoe UI", 10) };

            btnSubmit = new Button { Text = "Submit Review", Location = new Point(20, 265), Size = new Size(360, 35), BackColor = Color.FromArgb(155, 89, 182), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnSubmit.Click += BtnSubmit_Click;

            this.Controls.AddRange(new Control[] { lblProduct, cmbProduct, lblRating, nudRating, lblComment, txtComment, btnSubmit });
        }

        private void BtnSubmit_Click(object? sender, EventArgs e)
        {
            if (cmbProduct.SelectedIndex < 0) return;
            var productId = _items[cmbProduct.SelectedIndex].ProductId;
            var (success, msg) = _reviewBL.AddReview(SessionManager.CurrentUser!.UserId, productId, _orderId, (int)nudRating.Value, txtComment.Text.Trim());
            MessageBox.Show(msg, success ? "Success" : "Error", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (success) this.Close();
        }
    }
}
