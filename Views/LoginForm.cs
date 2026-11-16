using AgriConnect.Controllers;
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
            Size = new Size(500, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9.5f);

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = Color.FromArgb(46, 125, 50)
            };

            lblHeaderTitle = new Label
            {
                Text = "🌱 AgriConnect Rwanda",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 20),
                AutoSize = true
            };

            lblHeaderSubtitle = new Label
            {
                Text = "Smart Farm Produce Marketplace & Advisory Platform",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(232, 245, 233),
                Location = new Point(28, 55),
                AutoSize = true
            };

            cmbLang = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(350, 20),
                Size = new Size(115, 26),
                Font = new Font("Segoe UI", 8.5f)
            };
            cmbLang.Items.AddRange(new object[] { "English", "Kinyarwanda", "Français" });
            cmbLang.SelectedIndex = 0;
            cmbLang.SelectedIndexChanged += (s, e) => ApplyLanguage(cmbLang.SelectedItem?.ToString() ?? "English");

            pnlTop.Controls.AddRange(new Control[] { lblHeaderTitle, lblHeaderSubtitle, cmbLang });

            int y = 140;

            lblEmailPhone = new Label
            {
                Text = "Email or Phone Number:",
                Location = new Point(45, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            y += 26;

            txtEmailPhone = new TextBox
            {
                Text = "farmer.jean@agriconnect.rw",
                Location = new Point(45, y),
                Size = new Size(395, 30),
                Font = new Font("Segoe UI", 10.5f)
            };
            y += 42;

            lblPassword = new Label
            {
                Text = "Password:",
                Location = new Point(45, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            y += 26;

            txtPassword = new TextBox
            {
                Text = "farmer123",
                PasswordChar = '●',
                Location = new Point(45, y),
                Size = new Size(395, 30),
                Font = new Font("Segoe UI", 10.5f)
            };
            y += 34;

            chkShowPass = new CheckBox
            {
                Text = "Show Password",
                Location = new Point(45, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f)
            };
            chkShowPass.CheckedChanged += (s, e) => txtPassword.PasswordChar = chkShowPass.Checked ? '\0' : '●';
            y += 34;

            btnLogin = new Button
            {
                Text = "Sign In",
                BackColor = Color.FromArgb(46, 125, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Location = new Point(45, y),
                Size = new Size(395, 42),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            y += 50;

            btnRegister = new Button
            {
                Text = "Don't have an account? Register Here",
                BackColor = Color.FromArgb(245, 245, 245),
                ForeColor = Color.FromArgb(46, 125, 50),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(45, y),
                Size = new Size(395, 36),
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnRegister.Click += BtnRegister_Click;
            y += 50;

            // Quick Demo Accounts Bar
            lblQuickDemo = new Label
            {
                Text = "⚡ Instant Demo Login (Click any profile below):",
                Location = new Point(45, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.Gray
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
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                    Size = new Size(76, 26),
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

            AddDemoButton("Admin", "admin@agriconnect.rw", "admin123", Color.FromArgb(66, 66, 66));
            AddDemoButton("Farmer", "farmer.jean@agriconnect.rw", "farmer123", Color.FromArgb(46, 125, 50));
            AddDemoButton("Buyer", "buyer.kigali@agriconnect.rw", "buyer123", Color.FromArgb(21, 101, 192));
            AddDemoButton("Transporter", "transporter.eric@agriconnect.rw", "driver123", Color.FromArgb(230, 81, 0));
            AddDemoButton("Agronomist", "agronomist.alice@agriconnect.rw", "agri123", Color.FromArgb(106, 27, 154));

            Controls.AddRange(new Control[]
            {
                pnlTop, lblEmailPhone, txtEmailPhone, lblPassword, txtPassword,
                chkShowPass, btnLogin, btnRegister, lblQuickDemo, pnlDemo
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
