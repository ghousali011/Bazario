using ECommerce.BL;
using ECommerce.Models.Enums;
using ECommerce.Utilities;

namespace ECommerce.UI.Forms
{
    public partial class RegisterForm : Form
    {
        private readonly UserBL _userBL = new();

        private TextBox txtName = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPhone = null!;
        private TextBox txtPassword = null!;
        private TextBox txtConfirmPassword = null!;
        private ComboBox cmbRole = null!;
        private Button btnRegister = null!;

        public RegisterForm()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            this.Text = "Create Account";
            this.Size = new Size(450, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            var lblTitle = new Label { Text = "Create Account", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80), Location = new Point(120, 15), AutoSize = true };

            int y = 55;
            var lblName = new Label { Text = "Full Name", Location = new Point(50, y), Font = new Font("Segoe UI", 9), AutoSize = true };
            txtName = new TextBox { Location = new Point(50, y + 22), Size = new Size(330, 28), Font = new Font("Segoe UI", 10) };

            y += 55;
            var lblEmail = new Label { Text = "Email", Location = new Point(50, y), Font = new Font("Segoe UI", 9), AutoSize = true };
            txtEmail = new TextBox { Location = new Point(50, y + 22), Size = new Size(330, 28), Font = new Font("Segoe UI", 10) };

            y += 55;
            var lblPhone = new Label { Text = "Phone (optional)", Location = new Point(50, y), Font = new Font("Segoe UI", 9), AutoSize = true };
            txtPhone = new TextBox { Location = new Point(50, y + 22), Size = new Size(330, 28), Font = new Font("Segoe UI", 10) };

            y += 55;
            var lblPass = new Label { Text = "Password", Location = new Point(50, y), Font = new Font("Segoe UI", 9), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(50, y + 22), Size = new Size(330, 28), Font = new Font("Segoe UI", 10), UseSystemPasswordChar = true };

            y += 55;
            var lblConfirm = new Label { Text = "Confirm Password", Location = new Point(50, y), Font = new Font("Segoe UI", 9), AutoSize = true };
            txtConfirmPassword = new TextBox { Location = new Point(50, y + 22), Size = new Size(330, 28), Font = new Font("Segoe UI", 10), UseSystemPasswordChar = true };

            y += 55;
            var lblRole = new Label { Text = "Register as", Location = new Point(50, y), Font = new Font("Segoe UI", 9), AutoSize = true };
            cmbRole = new ComboBox { Location = new Point(50, y + 22), Size = new Size(330, 28), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new object[] { "Customer", "Seller" });
            cmbRole.SelectedIndex = 0;

            y += 60;
            btnRegister = new Button { Text = "Register", Location = new Point(50, y), Size = new Size(330, 40), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;

            this.Controls.AddRange(new Control[] { lblTitle, lblName, txtName, lblEmail, txtEmail, lblPhone, txtPhone, lblPass, txtPassword, lblConfirm, txtConfirmPassword, lblRole, cmbRole, btnRegister });
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            var role = cmbRole.SelectedIndex == 0 ? UserRole.Customer : UserRole.Seller;

            var (success, message, userId) = _userBL.Register(
                txtName.Text.Trim(), txtEmail.Text.Trim(),
                txtPassword.Text, txtConfirmPassword.Text,
                txtPhone.Text.Trim(), role);

            if (!success)
            {
                MessageBox.Show(message, "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Logger.LogInfo($"New registration: {txtEmail.Text} as {role}");

            // Show OTP verification
            var otpForm = new OtpVerificationForm(txtEmail.Text.Trim());
            this.Hide();
            var result = otpForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                MessageBox.Show("Account verified! You can now sign in.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            this.Close();
        }
    }
}
