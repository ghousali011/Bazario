using ECommerce.BL;
using ECommerce.Models;

namespace ECommerce.UI.Forms
{
    public partial class ProductForm : Form
    {
        private readonly ProductBL _productBL = new();
        private readonly Product? _existingProduct;

        private TextBox txtName = null!;
        private TextBox txtDescription = null!;
        private NumericUpDown nudPrice = null!;
        private NumericUpDown nudDiscount = null!;
        private NumericUpDown nudStock = null!;
        private ComboBox cmbCategory = null!;
        private TextBox txtImage = null!;
        private Button btnSave = null!;

        public ProductForm(Product? product = null)
        {
            _existingProduct = product;
            InitializeComponent();
            if (product != null) PopulateFields(product);
        }

        private void InitializeComponent()
        {
            this.Text = _existingProduct == null ? "Add Product" : "Edit Product";
            this.Size = new Size(450, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            int y = 15;
            AddField("Product Name", ref y, out txtName);
            AddField("Description", ref y, out txtDescription, true);

            var lblCat = new Label { Text = "Category", Location = new Point(20, y), AutoSize = true };
            cmbCategory = new ComboBox { Location = new Point(20, y + 22), Size = new Size(390, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            var categories = _productBL.GetAllCategories();
            foreach (var c in categories) cmbCategory.Items.Add(c.CategoryName);
            if (cmbCategory.Items.Count > 0) cmbCategory.SelectedIndex = 0;
            cmbCategory.Tag = categories;
            y += 55;

            var lblPrice = new Label { Text = "Price ($)", Location = new Point(20, y), AutoSize = true };
            nudPrice = new NumericUpDown { Location = new Point(20, y + 22), Size = new Size(180, 28), Maximum = 999999, DecimalPlaces = 2 };
            var lblDisc = new Label { Text = "Discount Price ($)", Location = new Point(220, y), AutoSize = true };
            nudDiscount = new NumericUpDown { Location = new Point(220, y + 22), Size = new Size(190, 28), Maximum = 999999, DecimalPlaces = 2 };
            y += 55;

            var lblStock = new Label { Text = "Stock Quantity", Location = new Point(20, y), AutoSize = true };
            nudStock = new NumericUpDown { Location = new Point(20, y + 22), Size = new Size(180, 28), Maximum = 999999 };
            y += 55;

            AddField("Image URL (optional)", ref y, out txtImage);

            btnSave = new Button { Text = "Save Product", Location = new Point(20, y), Size = new Size(390, 40), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] { lblCat, cmbCategory, lblPrice, nudPrice, lblDisc, nudDiscount, lblStock, nudStock, btnSave });
        }

        private void AddField(string label, ref int y, out TextBox txt, bool multiline = false)
        {
            var lbl = new Label { Text = label, Location = new Point(20, y), AutoSize = true };
            txt = new TextBox { Location = new Point(20, y + 22), Size = new Size(390, multiline ? 50 : 28), Multiline = multiline };
            this.Controls.AddRange(new Control[] { lbl, txt });
            y += multiline ? 80 : 55;
        }

        private void PopulateFields(Product p)
        {
            txtName.Text = p.ProductName;
            txtDescription.Text = p.Description;
            nudPrice.Value = p.Price;
            nudDiscount.Value = p.DiscountPrice ?? 0;
            nudStock.Value = p.StockQuantity;
            txtImage.Text = p.ImageUrl ?? "";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var categories = (List<Category>)cmbCategory.Tag!;
            if (cmbCategory.SelectedIndex < 0) { MessageBox.Show("Select a category."); return; }

            var product = _existingProduct ?? new Product();
            product.ProductName = txtName.Text.Trim();
            product.Description = txtDescription.Text.Trim();
            product.Price = nudPrice.Value;
            product.DiscountPrice = nudDiscount.Value > 0 ? nudDiscount.Value : null;
            product.StockQuantity = (int)nudStock.Value;
            product.CategoryId = categories[cmbCategory.SelectedIndex].CategoryId;
            product.ImageUrl = string.IsNullOrWhiteSpace(txtImage.Text) ? null : txtImage.Text.Trim();
            product.SellerId = SessionManager.CurrentUser!.UserId;

            if (_existingProduct != null)
            {
                _productBL.UpdateProduct(product);
                MessageBox.Show("Product updated!");
            }
            else
            {
                var (success, msg, _) = _productBL.AddProduct(product);
                MessageBox.Show(msg);
                if (!success) return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
