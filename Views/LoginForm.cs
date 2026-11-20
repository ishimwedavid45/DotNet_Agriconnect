using AgriConnect.Controllers;
using AgriConnect.Helpers;
using AgriConnect.Models;

namespace AgriConnect.Views
{
    public class LoginForm : Form
    {
        private readonly AuthController _auth = new();

        private ComboBox cmbLang = null!;
        private Label lblHeaderTitle = null!;
        private Label lblHeaderSubtitle = null!;
        private Label lblEmailPhone = null!;
        private TextBox txtEmailPhone = null!;
        private Label lblPassword = null!;
        private TextBox txtPassword = null!;
        private CheckBox chkShowPass = null!;
        private Button btnLogin = null!;
        private Button btnRegister = null!;
        private Label lblQuickDemo = null!;

        public LoginForm()
        {
            InitializeComponent();
            ApplyLanguage("English");
        }

        private void InitializeComponent()
        {
            Text = "AgriConnect - Sign In";
            Size = new Size(500, 610);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = AppTheme.White;
            Font = new Font("Segoe UI", 9.5f);

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = AppTheme.Charcoal
            };

            lblHeaderTitle = new Label
            {
                Text = "🌱 AgriConnect Rwanda",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = AppTheme.White,
                Location = new Point(25, 20),
                AutoSize = true
            };

            lblHeaderSubtitle = new Label
            {
                Text = "Smart Farm Produce Marketplace & Advisory Platform",
                Font = new Font("Segoe UI", 9f),
                ForeColor = AppTheme.SlateBorder,
                Location = new Point(28, 55),
                AutoSize = true
            };

            cmbLang = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(350, 20),
                Size = new Size(115, 26),
                Font = new Font("Segoe UI", 8.5f),
                BackColor = AppTheme.DarkGrey,
                ForeColor = AppTheme.White
            };
            cmbLang.Items.AddRange(new object[] { "English", "Kinyarwanda", "Français" });
            cmbLang.SelectedIndex = 0;
            cmbLang.SelectedIndexChanged += (s, e) => ApplyLanguage(cmbLang.SelectedItem?.ToString() ?? "English");

            pnlTop.Controls.AddRange(new Control[] { lblHeaderTitle, lblHeaderSubtitle, cmbLang });

            int y = 135;

            lblEmailPhone = new Label
            {
                Text = "Email or Phone Number:",
                Location = new Point(45, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary
            };
            y += 26;

            txtEmailPhone = new TextBox
            {
                Text = "farmer.jean@agriconnect.rw",
                Location = new Point(45, y),
                Size = new Size(395, 30),
                Font = new Font("Segoe UI", 10.5f),
                BackColor = AppTheme.OffWhite,
                ForeColor = AppTheme.TextPrimary
            };
            y += 42;

            lblPassword = new Label
            {
                Text = "Password:",
                Location = new Point(45, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary
            };
            y += 26;

            txtPassword = new TextBox
            {
                Text = "farmer123",
                PasswordChar = '●',
                Location = new Point(45, y),
                Size = new Size(395, 30),
                Font = new Font("Segoe UI", 10.5f),
                BackColor = AppTheme.OffWhite,
                ForeColor = AppTheme.TextPrimary
            };
            y += 34;

            chkShowPass = new CheckBox
            {
                Text = "Show Password",
                Location = new Point(45, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = AppTheme.TextSecondary
            };
            chkShowPass.CheckedChanged += (s, e) => txtPassword.PasswordChar = chkShowPass.Checked ? '\0' : '●';
            y += 32;

            btnLogin = new Button
            {
                Text = "Sign In",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Location = new Point(45, y),
                Size = new Size(395, 42)
            };
            AppTheme.StylePrimaryButton(btnLogin);
            btnLogin.Click += BtnLogin_Click;
            y += 50;

            btnRegister = new Button
            {
                Text = "Don't have an account? Register Here",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(45, y),
                Size = new Size(395, 36)
            };
            AppTheme.StyleSecondaryButton(btnRegister);
            btnRegister.Click += BtnRegister_Click;
            y += 52;

            // Quick Demo Accounts Bar
            lblQuickDemo = new Label
            {
                Text = "⚡ Instant Demo Login (Click any profile below):",
                Location = new Point(45, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = AppTheme.TextSecondary
            };
            y += 22;

            var pnlDemo = new FlowLayoutPanel
            {
                Location = new Point(40, y),
                Size = new Size(410, 60),
                FlowDirection = FlowDirection.LeftToRight
            };

            void AddDemoButton(string label, string email, string pass, Color color)
            {
                var b = new Button
                {
                    Text = label,
                    BackColor = color,
                    ForeColor = AppTheme.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                    Size = new Size(76, 28),
                    Margin = new Padding(2),
                    Cursor = Cursors.Hand
                };
                b.FlatAppearance.BorderSize = 0;
                b.Click += (s, e) =>
                {
                    txtEmailPhone.Text = email;
                    txtPassword.Text = pass;
                    BtnLogin_Click(btnLogin, EventArgs.Empty);
                };
                pnlDemo.Controls.Add(b);
            }

            AddDemoButton("Admin", "admin@agriconnect.rw", "admin123", AppTheme.Charcoal);
            AddDemoButton("Farmer", "farmer.jean@agriconnect.rw", "farmer123", AppTheme.DarkGrey);
            AddDemoButton("Buyer", "buyer.kigali@agriconnect.rw", "buyer123", AppTheme.MediumGrey);
            AddDemoButton("Transporter", "transporter.eric@agriconnect.rw", "driver123", AppTheme.MutedGrey);
            AddDemoButton("Agronomist", "agronomist.alice@agriconnect.rw", "agri123", AppTheme.DeepCharcoal);

            var btnOpenForm1 = new Button
            {
                Text = "🌾 Open Produce Manager (Form1)",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location = new Point(45, y + 66),
                Size = new Size(395, 32),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
            };
            AppTheme.StyleSecondaryButton(btnOpenForm1);
            btnOpenForm1.Click += (s, e) =>
            {
                var f1 = new Form1();
                f1.Show();
            };

            Controls.AddRange(new Control[]
            {
                pnlTop, lblEmailPhone, txtEmailPhone, lblPassword, txtPassword,
                chkShowPass, btnLogin, btnRegister, lblQuickDemo, pnlDemo, btnOpenForm1
            });

            AcceptButton = btnLogin;
        }

        private void ApplyLanguage(string lang)
        {
            AppSession.CurrentLanguage = lang;
            if (lang == "Kinyarwanda")
            {
                lblHeaderSubtitle.Text = "Isoko ry'imyaka n'ubujyanama ku bahinzi n'abaguzi";
                lblEmailPhone.Text = "Imeli cyangwa Nimero ya Telefoni:";
                lblPassword.Text = "Ijambobanga (Password):";
                chkShowPass.Text = "Erekana Ijambobanga";
                btnLogin.Text = "Injira muri Sisitemu";
                btnRegister.Text = "Nta konti ufite? Iyandikishe hano";
                lblQuickDemo.Text = "⚡ Kanda hano winjire ako kanya (Demo):";
            }
            else if (lang == "Français")
            {
                lblHeaderSubtitle.Text = "Marché des produits agricoles et conseils aux agriculteurs";
                lblEmailPhone.Text = "Email ou Numéro de Téléphone :";
                lblPassword.Text = "Mot de passe :";
                chkShowPass.Text = "Afficher le mot de passe";
                btnLogin.Text = "Se Connecter";
                btnRegister.Text = "Pas encore de compte ? S'inscrire";
                lblQuickDemo.Text = "⚡ Connexion Démo Rapide (Cliquez ci-dessous) :";
            }
            else
            {
                lblHeaderSubtitle.Text = "Smart Farm Produce Marketplace & Advisory Platform";
                lblEmailPhone.Text = "Email or Phone Number:";
                lblPassword.Text = "Password:";
                chkShowPass.Text = "Show Password";
                btnLogin.Text = "Sign In";
                btnRegister.Text = "Don't have an account? Register Here";
                lblQuickDemo.Text = "⚡ Instant Demo Login (Click any profile below):";
            }
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            var (success, msg, user) = _auth.Login(txtEmailPhone.Text, txtPassword.Text);
            if (success && user != null)
            {
                Hide();
                var mainForm = new MainDashboardForm();
                mainForm.FormClosed += (s, args) =>
                {
                    if (AppSession.IsLoggedIn)
                    {
                        Close();
                    }
                    else
                    {
                        Show();
                        txtPassword.Clear();
                    }
                };
                mainForm.Show();
            }
            else
            {
                MessageBox.Show(msg, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            using var reg = new RegisterForm();
            if (reg.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Registration successful! You may now sign in.", "Welcome", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtEmailPhone.Text = reg.RegisteredEmailOrPhone;
                txtPassword.Clear();
            }
        }
    }
}
