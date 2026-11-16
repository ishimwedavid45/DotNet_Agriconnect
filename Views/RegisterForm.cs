using AgriConnect.Controllers;
using AgriConnect.Models;

namespace AgriConnect.Views
{
    public class RegisterForm : Form
    {
        private readonly AuthController _auth = new();
        public string RegisteredEmailOrPhone { get; private set; } = string.Empty;

        private ComboBox cmbRole = null!;
        private TextBox txtName = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPhone = null!;
        private TextBox txtPassword = null!;
        private ComboBox cmbDistrict = null!;
        private TextBox txtSector = null!;

        // Farmer specifics
        private Label lblFarmSize = null!;
        private TextBox txtFarmSize = null!;
        private Label lblCrops = null!;
        private TextBox txtCrops = null!;
        private Label lblCoop = null!;
        private TextBox txtCoop = null!;

        // Buyer specifics
        private Label lblDelivery = null!;
        private TextBox txtDelivery = null!;

        public RegisterForm()
        {
            InitializeComponent();
            UpdateRoleFields();
        }

        private void InitializeComponent()
        {
            Text = "AgriConnect - Account Registration";
            Size = new Size(520, 680);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9.5f);

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(46, 125, 50)
            };
            var lblTitle = new Label
            {
                Text = "🌱 Join AgriConnect Rwanda",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 12),
                AutoSize = true
            };
            var lblSub = new Label
            {
                Text = "Connecting farmers, buyers, transporters & agronomists directly",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(232, 245, 233),
                Location = new Point(22, 38),
                AutoSize = true
            };
            pnlTop.Controls.AddRange(new Control[] { lblTitle, lblSub });

            var pnlBody = new Panel
            {
                Location = new Point(20, 80),
                Size = new Size(465, 500),
                AutoScroll = true
            };

            int y = 5;
            void AddField(string label, Control ctrl, int height = 26)
            {
                var l = new Label
                {
                    Text = label,
                    Location = new Point(10, y),
                    Size = new Size(160, 22),
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 50, 50)
                };
                ctrl.Location = new Point(180, y);
                ctrl.Size = new Size(260, height);
                pnlBody.Controls.Add(l);
                pnlBody.Controls.Add(ctrl);
                y += height + 10;
            }

            cmbRole = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new object[] { "Farmer", "Buyer", "Transporter" });
            cmbRole.SelectedIndex = 0;
            cmbRole.SelectedIndexChanged += (s, e) => UpdateRoleFields();
            AddField("Account Type *", cmbRole);

            txtName = new TextBox();
            AddField("Full Name *", txtName);

            txtEmail = new TextBox();
            AddField("Email Address", txtEmail);

            txtPhone = new TextBox { Text = "+25078" };
            AddField("Phone (MoMo) *", txtPhone);

            txtPassword = new TextBox { PasswordChar = '●' };
            AddField("Password *", txtPassword);

            cmbDistrict = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDistrict.Items.AddRange(new object[] { "Gasabo", "Kicukiro", "Nyarugenge", "Musanze", "Nyagatare", "Rubavu", "Huye", "Bugesera", "Rwamagana" });
            cmbDistrict.SelectedIndex = 0;
            AddField("District *", cmbDistrict);

            txtSector = new TextBox();
            AddField("Sector *", txtSector);

            // Farmer custom controls
            lblFarmSize = new Label { Text = "Farm Size (ha):", Location = new Point(10, y), Size = new Size(160, 22), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            txtFarmSize = new TextBox { Location = new Point(180, y), Size = new Size(260, 26) };
            pnlBody.Controls.AddRange(new Control[] { lblFarmSize, txtFarmSize });
            y += 36;

            lblCrops = new Label { Text = "Primary Crops:", Location = new Point(10, y), Size = new Size(160, 22), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            txtCrops = new TextBox { Location = new Point(180, y), Size = new Size(260, 26) };
            pnlBody.Controls.AddRange(new Control[] { lblCrops, txtCrops });
            y += 36;

            lblCoop = new Label { Text = "Cooperative Name:", Location = new Point(10, y), Size = new Size(160, 22), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            txtCoop = new TextBox { Location = new Point(180, y), Size = new Size(260, 26) };
            pnlBody.Controls.AddRange(new Control[] { lblCoop, txtCoop });
            y += 36;

            // Buyer custom controls
            lblDelivery = new Label { Text = "Delivery Address:", Location = new Point(10, y), Size = new Size(160, 22), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            txtDelivery = new TextBox { Location = new Point(180, y), Size = new Size(260, 26) };
            pnlBody.Controls.AddRange(new Control[] { lblDelivery, txtDelivery });
            y += 36;

            var btnSubmit = new Button
            {
                Text = "Register Account",
                BackColor = Color.FromArgb(46, 125, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Location = new Point(150, 590),
                Size = new Size(180, 38),
                Cursor = Cursors.Hand
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += BtnSubmit_Click;

            var btnCancel = new Button
            {
                Text = "Cancel",
                BackColor = Color.FromArgb(235, 235, 235),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(340, 590),
                Size = new Size(110, 38)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] { pnlTop, pnlBody, btnSubmit, btnCancel });
        }

        private void UpdateRoleFields()
        {
            string role = cmbRole.SelectedItem?.ToString() ?? "Farmer";
            bool isFarmer = role == "Farmer";
            bool isBuyer = role == "Buyer";

            lblFarmSize.Visible = isFarmer;
            txtFarmSize.Visible = isFarmer;
            lblCrops.Visible = isFarmer;
            txtCrops.Visible = isFarmer;
            lblCoop.Visible = isFarmer;
            txtCoop.Visible = isFarmer;

            lblDelivery.Visible = isBuyer;
            txtDelivery.Visible = isBuyer;
        }

        private void BtnSubmit_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please fill all required fields.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // FR-1.2: One-Time Password (OTP) Verification Dialog
            string generatedOtp = new Random().Next(100000, 999999).ToString();
            using var otpDlg = new OtpVerificationForm(txtPhone.Text, generatedOtp);
            if (otpDlg.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show("OTP verification was cancelled. Account registration aborted.", "Verification Aborted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var role = cmbRole.SelectedIndex switch
            {
                1 => UserRole.Buyer,
                2 => UserRole.Transporter,
                _ => UserRole.Farmer
            };

            var (success, msg, user) = _auth.Register(
                txtName.Text,
                string.IsNullOrWhiteSpace(txtEmail.Text) ? $"{txtPhone.Text.Replace("+","")}@agriconnect.local" : txtEmail.Text,
                txtPhone.Text,
                txtPassword.Text,
                role,
                cmbDistrict.SelectedItem?.ToString() ?? "Gasabo",
                txtSector.Text.Trim(),
                txtFarmSize.Text.Trim(),
                txtCrops.Text.Trim(),
                txtCoop.Text.Trim(),
                txtDelivery.Text.Trim()
            );

            if (success && user != null)
            {
                RegisteredEmailOrPhone = user.Phone;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(msg, "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    public class OtpVerificationForm : Form
    {
        private readonly string _expectedOtp;
        private TextBox txtInput = null!;

        public OtpVerificationForm(string phone, string expectedOtp)
        {
            _expectedOtp = expectedOtp;
            Text = "AgriConnect OTP Verification (FR-1.2)";
            Size = new Size(420, 260);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9.5f);

            var lblHeader = new Label
            {
                Text = "📲 One-Time Password Verification",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 125, 50),
                Location = new Point(20, 15),
                AutoSize = true
            };

            var lblInfo = new Label
            {
                Text = $"We simulated sending an SMS OTP to {phone}.\nFor test verification, your security code is: {_expectedOtp}",
                Location = new Point(20, 45),
                Size = new Size(360, 45)
            };

            var lblEnter = new Label
            {
                Text = "Enter 6-digit OTP:",
                Location = new Point(20, 100),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            txtInput = new TextBox
            {
                Text = _expectedOtp, // pre-fill for ease of testing
                Location = new Point(20, 125),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Center
            };

            var btnVerify = new Button
            {
                Text = "Verify & Activate",
                BackColor = Color.FromArgb(46, 125, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Location = new Point(140, 175),
                Size = new Size(130, 34)
            };
            btnVerify.FlatAppearance.BorderSize = 0;
            btnVerify.Click += (s, e) =>
            {
                if (txtInput.Text.Trim() == _expectedOtp)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Incorrect OTP code. Please enter the valid 6-digit code.", "Verification Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                BackColor = Color.FromArgb(235, 235, 235),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(280, 175),
                Size = new Size(100, 34)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.AddRange(new Control[] { lblHeader, lblInfo, lblEnter, txtInput, btnVerify, btnCancel });
            AcceptButton = btnVerify;
        }
    }
}
