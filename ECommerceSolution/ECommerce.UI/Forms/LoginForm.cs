using ECommerce.BL;
using ECommerce.Utilities;

namespace ECommerce.UI.Forms
{
    public partial class LoginForm : Form
    {
        private readonly UserBL _userBL = new();

        private TextBox txtEmail = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Button btnRegister = null!;
        private Label lblTitle = null!;
        private Label lblEmail = null!;
        private Label lblPassword = null!;
        private LinkLabel lnkForgotPassword = null!;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "ECommerce - Sign In";
            this.Size = new Size(450, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            lblTitle = new Label { Text = "Welcome Back", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80), Location = new Point(120, 20), AutoSize = true };
            lblEmail = new Label { Text = "Email Address", Location = new Point(50, 80), Font = new Font("Segoe UI", 10), AutoSize = true };
            txtEmail = new TextBox { Location = new Point(50, 105), Size = new Size(330, 30), Font = new Font("Segoe UI", 11) };
            lblPassword = new Label { Text = "Password", Location = new Point(50, 145), Font = new Font("Segoe UI", 10), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(50, 170), Size = new Size(330, 30), Font = new Font("Segoe UI", 11), UseSystemPasswordChar = true };
            btnLogin = new Button { Text = "Sign In", Location = new Point(50, 220), Size = new Size(330, 40), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            btnRegister = new Button { Text = "Create New Account", Location = new Point(50, 270), Size = new Size(330, 40), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;
            lnkForgotPassword = new LinkLabel { Text = "Forgot Password?", Location = new Point(165, 320), AutoSize = true, Font = new Font("Segoe UI", 9) };

            this.Controls.AddRange(new Control[] { lblTitle, lblEmail, txtEmail, lblPassword, txtPassword, btnLogin, btnRegister, lnkForgotPassword });
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            var (success, message, user) = _userBL.Login(txtEmail.Text.Trim(), txtPassword.Text);
            if (!success)
            {
                MessageBox.Show(message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.LogWarning($"Failed login attempt for: {txtEmail.Text}");
                return;
            }

            SessionManager.CurrentUser = user;
            Logger.LogInfo($"User logged in: {user!.Email} (Role: {user.Role})");

            Form dashboard = user.Role switch
            {
                Models.Enums.UserRole.Customer => new CustomerDashboardForm(),
                Models.Enums.UserRole.Seller => new SellerDashboardForm(),
                Models.Enums.UserRole.Administrator => new AdminDashboardForm(),
                _ => throw new InvalidOperationException("Unknown role")
            };

            this.Hide();
            dashboard.FormClosed += (s, args) => { SessionManager.Logout(); this.Show(); txtPassword.Clear(); };
            dashboard.Show();
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            var registerForm = new RegisterForm();
            registerForm.ShowDialog();
        }
    }
}
