using ECommerce.BL;
using ECommerce.Models;
using System.Windows.Forms;

namespace ECommerce.UI.Forms
{
    public class OrderDetailsForm : Form
    {
        private readonly Order _order;
        private DataGridView dgvItems = null!;

        public OrderDetailsForm(Order order)
        {
            _order = order;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = $"Order #{_order.OrderId} Details";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            var lblInfo = new Label { Text = $"Customer: {_order.CustomerName}    Date: {_order.OrderDate:g}    Total: ${_order.TotalAmount:F2}", AutoSize = true, Location = new Point(10, 10) };
            dgvItems = new DataGridView
            {
                Location = new Point(10, 40),
                Size = new Size(560, 300),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowHeadersVisible = false
            };

            this.Controls.Add(lblInfo);
            this.Controls.Add(dgvItems);
        }

        private void LoadData()
        {
            dgvItems.DataSource = _order.Items.Select(i => new
            {
                i.ProductId,
                i.ProductName,
                i.Quantity,
                UnitPrice = $"${i.UnitPrice:F2}",
                Total = $"${i.TotalPrice:F2}",
                Seller = i.SellerName
            }).ToList();
        }
    }
}
