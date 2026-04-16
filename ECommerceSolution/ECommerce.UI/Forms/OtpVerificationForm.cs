using ECommerce.BL;

namespace ECommerce.UI.Forms
{
    public partial class OtpVerificationForm : Form
    {
        private readonly UserBL _userBL = new();
        private readonly string _email;

        private TextBox txtOtp = null!;
        private Button btnVerify = null!;
        private LinkLabel lnkResend = null!;

        public OtpVerificationForm(string email)
        {
            _email = email;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Verify Email";
            this.Size = new Size(400, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            var lblTitle = new Label { Text = "Email Verification", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80), Location = new Point(80, 15), AutoSize = true };
            var lblInfo = new Label { Text = $"Enter the OTP sent to {_email}", Location = new Point(50, 55), Size = new Size(300, 20), Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleCenter };
            txtOtp = new TextBox { Location = new Point(100, 90), Size = new Size(180, 35), Font = new Font("Segoe UI", 16), TextAlign = HorizontalAlignment.Center, MaxLength = 6 };
            btnVerify = new Button { Text = "Verify", Location = new Point(100, 140), Size = new Size(180, 40), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnVerify.FlatAppearance.BorderSize = 0;
            btnVerify.Click += BtnVerify_Click;
            lnkResend = new LinkLabel { Text = "Resend OTP", Location = new Point(145, 195), AutoSize = true, Font = new Font("Segoe UI", 9) };
            lnkResend.Click += LnkResend_Click;

            this.Controls.AddRange(new Control[] { lblTitle, lblInfo, txtOtp, btnVerify, lnkResend });
        }

        private void BtnVerify_Click(object? sender, EventArgs e)
        {
            var (success, message) = _userBL.VerifyOtp(_email, txtOtp.Text.Trim());
            if (success)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(message, "Verification Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LnkResend_Click(object? sender, EventArgs e)
        {
            var (success, message) = _userBL.ResendOtp(_email);
            MessageBox.Show(message, success ? "OTP Sent" : "Error", MessageBoxButtons.OK,
                success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
    }
}
