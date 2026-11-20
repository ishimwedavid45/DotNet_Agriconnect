using AgriConnect.Controllers;
using AgriConnect.Helpers;
using AgriConnect.Models;
using MySqlConnector;

namespace AgriConnect.Views
{
    /// <summary>
    /// Form1 — AgriConnect Produce Inventory & Marketplace Manager (MVC View).
    /// Connected directly to the MySQL database `agriconnect_db` running in XAMPP.
    /// All database operations use safe parameterized queries.
    /// Professional grey and white theme.
    /// </summary>
    public class Form1 : Form
    {
        // ── Controls ─────────────────────────────────────────────────────
        private Panel        _pnlHeader    = null!;
        private Panel        _pnlInput     = null!;
        private TextBox      txtProduceName= null!;
        private ComboBox     cmbCategory   = null!;
        private TextBox      txtQuantity   = null!;
        private TextBox      txtPrice      = null!;
        private TextBox      txtDistrict   = null!;
        private Button       btnSave       = null!;
        private Button       btnRefresh    = null!;
        private Button       btnDashboard  = null!;
        private Label        lblStatus     = null!;
        private DataGridView dgvListings   = null!;

        public Form1()
        {
            InitializeComponent();
            ApplyTheme();
            LoadListingsFromMySql();
        }

        private void InitializeComponent()
        {
            Text          = "AgriConnect – Produce & Inventory Manager (MySQL: agriconnect_db)";
            Size          = new Size(1000, 640);
            MinimumSize   = new Size(820, 520);
            StartPosition = FormStartPosition.CenterScreen;
            Font          = new Font("Segoe UI", 9.5f);

            // ── 1. Header ───────────────────────────────────────────────
            _pnlHeader = new Panel
            {
                Dock   = DockStyle.Top,
                Height = 60,
            };

            var lblTitle = new Label
            {
                Text      = "🌾  AgriConnect  –  Produce & Crop Inventory (MySQL agriconnect_db)",
                Dock      = DockStyle.Left,
                Width     = 650,
                TextAlign = ContentAlignment.MiddleLeft,
                Font      = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                Padding   = new Padding(16, 0, 0, 0),
            };

            btnDashboard = new Button
            {
                Text      = "🚀 Open Full Portal",
                Dock      = DockStyle.Right,
                Width     = 180,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
            };
            btnDashboard.FlatAppearance.BorderSize = 0;
            btnDashboard.Click += (s, e) =>
            {
                var login = new LoginForm();
                login.Show();
            };

            _pnlHeader.Controls.Add(lblTitle);
            _pnlHeader.Controls.Add(btnDashboard);

            // ── 2. Input Panel ──────────────────────────────────────────
            _pnlInput = new Panel
            {
                Dock    = DockStyle.Top,
                Height  = 125,
                Padding = new Padding(16, 10, 16, 10),
            };

            // Row 1: Produce Name, Category, Quantity
            var lblProduce = MakeLabel("Produce / Crop:", 16, 12);
            txtProduceName = MakeTextBox(130, 10, 210, "e.g. Irish Potatoes (Kinigi)");

            var lblCat = MakeLabel("Category:", 360, 12);
            cmbCategory = new ComboBox
            {
                Location      = new Point(430, 10),
                Size          = new Size(160, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            cmbCategory.Items.AddRange(new object[] { "Tubers", "Cereals", "Vegetables", "Fruits", "Legumes", "Coffee/Tea" });
            cmbCategory.SelectedIndex = 0;

            var lblQty = MakeLabel("Quantity (kg):", 610, 12);
            txtQuantity = MakeTextBox(705, 10, 120, "e.g. 1000");

            // Row 2: Price per kg, District, Buttons
            var lblPrice = MakeLabel("Price / kg (RWF):", 16, 56);
            txtPrice = MakeTextBox(130, 54, 120, "e.g. 480");

            var lblDist = MakeLabel("District:", 270, 56);
            txtDistrict = MakeTextBox(335, 54, 150, "e.g. Musanze");

            btnSave = new Button
            {
                Text      = "💾  Save Produce",
                Location  = new Point(510, 50),
                Size      = new Size(160, 36),
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
            };
            btnSave.Click += BtnSave_Click;

            btnRefresh = new Button
            {
                Text      = "🔄  Refresh",
                Location  = new Point(685, 50),
                Size      = new Size(110, 36),
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
            };
            btnRefresh.Click += (s, e) => LoadListingsFromMySql();

            _pnlInput.Controls.AddRange(new Control[]
            {
                lblProduce, txtProduceName,
                lblCat, cmbCategory,
                lblQty, txtQuantity,
                lblPrice, txtPrice,
                lblDist, txtDistrict,
                btnSave, btnRefresh
            });

            // ── 3. Status Bar ───────────────────────────────────────────
            lblStatus = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(16, 0, 0, 0),
                Text      = "Connecting to MySQL agriconnect_db on localhost:3306...",
            };

            // ── 4. DataGridView ─────────────────────────────────────────
            dgvListings = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible     = false,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                BorderStyle           = BorderStyle.None,
            };

            // Reverse Dock order
            Controls.Add(dgvListings);
            Controls.Add(lblStatus);
            Controls.Add(_pnlInput);
            Controls.Add(_pnlHeader);
        }

        private static Label MakeLabel(string text, int x, int y) => new Label
        {
            Text     = text,
            Location = new Point(x, y + 4),
            AutoSize = true,
            Font     = new Font("Segoe UI", 9f, FontStyle.Bold),
        };

        private static TextBox MakeTextBox(int x, int y, int width, string placeholder = "") => new TextBox
        {
            Location    = new Point(x, y),
            Size        = new Size(width, 28),
            MaxLength   = 150,
            PlaceholderText = placeholder,
        };

        private void ApplyTheme()
        {
            BackColor = AppTheme.OffWhite;
            ForeColor = AppTheme.Charcoal;

            // Header: Dark Grey + White text
            _pnlHeader.BackColor = AppTheme.DarkGrey;
            _pnlHeader.ForeColor = Color.White;
            foreach (Control c in _pnlHeader.Controls)
                c.ForeColor = Color.White;

            btnDashboard.BackColor = AppTheme.MediumGrey;
            btnDashboard.ForeColor = Color.White;

            // Input panel: Light Grey background
            _pnlInput.BackColor = AppTheme.LightGrey;

            // Status bar: Slate / Medium Grey
            lblStatus.BackColor = AppTheme.MediumGrey;
            lblStatus.ForeColor = AppTheme.OffWhite;

            // Save button: Dark Grey primary
            btnSave.BackColor = AppTheme.DarkGrey;
            btnSave.ForeColor = Color.White;
            btnSave.FlatAppearance.BorderSize = 0;

            // Refresh button: White secondary with border
            btnRefresh.BackColor = AppTheme.White;
            btnRefresh.ForeColor = AppTheme.Charcoal;
            btnRefresh.FlatAppearance.BorderColor = AppTheme.SlateBorder;

            // DataGridView: Professional Grey & White styling
            dgvListings.BackgroundColor                          = AppTheme.OffWhite;
            dgvListings.GridColor                                 = AppTheme.BorderGrey;
            dgvListings.DefaultCellStyle.BackColor               = AppTheme.White;
            dgvListings.DefaultCellStyle.ForeColor               = AppTheme.Charcoal;
            dgvListings.DefaultCellStyle.SelectionBackColor      = AppTheme.DarkGrey;
            dgvListings.DefaultCellStyle.SelectionForeColor      = Color.White;
            dgvListings.AlternatingRowsDefaultCellStyle.BackColor = AppTheme.LightGrey;
            dgvListings.ColumnHeadersDefaultCellStyle.BackColor  = AppTheme.DarkGrey;
            dgvListings.ColumnHeadersDefaultCellStyle.ForeColor  = Color.White;
            dgvListings.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvListings.ColumnHeadersBorderStyle                = DataGridViewHeaderBorderStyle.None;
            dgvListings.EnableHeadersVisualStyles                = false;
        }

        // ── Save Produce Listing to MySQL using Parameterized Query ──────
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            lblStatus.ForeColor = AppTheme.OffWhite;

            // Validation
            string produce = txtProduceName.Text.Trim();
            if (string.IsNullOrWhiteSpace(produce))
            {
                ShowError("Please enter the Produce / Crop Name.");
                txtProduceName.Focus();
                return;
            }

            if (!decimal.TryParse(txtQuantity.Text.Trim(), out decimal quantity) || quantity <= 0)
            {
                ShowError("Please enter a valid positive Quantity in kg.");
                txtQuantity.Focus();
                return;
            }

            if (!decimal.TryParse(txtPrice.Text.Trim(), out decimal price) || price <= 0)
            {
                ShowError("Please enter a valid positive Price per kg.");
                txtPrice.Focus();
                return;
            }

            string category = cmbCategory.SelectedItem?.ToString() ?? "Vegetables";
            string district = string.IsNullOrWhiteSpace(txtDistrict.Text) ? "Musanze" : txtDistrict.Text.Trim();

            try
            {
                // Execute Parameterized INSERT Query directly into MySQL
                const string sql = @"
                    INSERT INTO listings 
                        (FarmerId, FarmerName, ProduceName, Category, Quantity, OriginalQuantity, Unit, PricePerUnit, HarvestDate, PickupLocation, District, Status, Description, ImageUrl, CreatedAt)
                    VALUES 
                        (@farmerId, @farmerName, @produce, @cat, @qty, @origQty, @unit, @price, @harvest, @pickup, @district, @status, @desc, @img, @created);";

                using var conn = MySqlConnectionFactory.CreateConnection();
                conn.Open();

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@farmerId",   2); // default demo farmer (Jean Claude)
                cmd.Parameters.AddWithValue("@farmerName", "Jean Claude Hakizimana");
                cmd.Parameters.AddWithValue("@produce",    produce);
                cmd.Parameters.AddWithValue("@cat",        category);
                cmd.Parameters.AddWithValue("@qty",        quantity);
                cmd.Parameters.AddWithValue("@origQty",    quantity);
                cmd.Parameters.AddWithValue("@unit",       "kg");
                cmd.Parameters.AddWithValue("@price",      price);
                cmd.Parameters.AddWithValue("@harvest",    DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@pickup",     $"{district} Agricultural Collection Center");
                cmd.Parameters.AddWithValue("@district",   district);
                cmd.Parameters.AddWithValue("@status",     "Active");
                cmd.Parameters.AddWithValue("@desc",       $"High quality fresh {produce} harvested in {district}.");
                cmd.Parameters.AddWithValue("@img",        "");
                cmd.Parameters.AddWithValue("@created",    DateTime.UtcNow);

                cmd.ExecuteNonQuery();

                // Clear input boxes
                txtProduceName.Clear();
                txtQuantity.Clear();
                txtPrice.Clear();
                txtDistrict.Clear();
                txtProduceName.Focus();

                // Reload and notify
                LoadListingsFromMySql();
                lblStatus.ForeColor = Color.LightGreen;
                lblStatus.Text = $"✅ Successfully saved '{produce}' to agriconnect_db.listings table!";
            }
            catch (MySqlException ex)
            {
                ShowError($"MySQL Error ({ex.Number}): {ex.Message}");
            }
            catch (Exception ex)
            {
                ShowError($"Unexpected Error: {ex.Message}");
            }
        }

        // ── Load Listings from MySQL and Bind to DataGridView ─────────────
        private void LoadListingsFromMySql()
        {
            try
            {
                var list = new List<Listing>();

                using var conn = MySqlConnectionFactory.CreateConnection();
                conn.Open();

                const string sql = "SELECT ListingId, FarmerName, ProduceName, Category, Quantity, PricePerUnit, District, Status, HarvestDate FROM listings ORDER BY ListingId DESC;";
                using var cmd = new MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Enum.TryParse<ListingStatus>(reader.GetString("Status"), out var status);
                    list.Add(new Listing
                    {
                        ListingId    = reader.GetInt32("ListingId"),
                        FarmerName   = reader.GetString("FarmerName"),
                        ProduceName  = reader.GetString("ProduceName"),
                        Category     = reader.GetString("Category"),
                        Quantity     = reader.GetDecimal("Quantity"),
                        PricePerUnit = reader.GetDecimal("PricePerUnit"),
                        District     = reader.GetString("District"),
                        Status       = status,
                        HarvestDate  = reader.IsDBNull(reader.GetOrdinal("HarvestDate")) ? DateTime.UtcNow : reader.GetDateTime("HarvestDate"),
                    });
                }

                BindGrid(list);

                lblStatus.ForeColor = AppTheme.OffWhite;
                lblStatus.Text = $"✅ Connected to MySQL: agriconnect_db (localhost:3306) | Total Produce Listings: {list.Count}";
            }
            catch (MySqlException ex)
            {
                ShowError($"MySQL Connection Failed: {ex.Message} — Ensure XAMPP MySQL is Running.");
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load listings: {ex.Message}");
            }
        }

        private void BindGrid(List<Listing> list)
        {
            dgvListings.DataSource = null;
            dgvListings.DataSource = list;

            if (dgvListings.Columns.Count > 0)
            {
                // Set friendly headers
                if (dgvListings.Columns["ListingId"] != null)
                {
                    dgvListings.Columns["ListingId"]!.HeaderText = "ID";
                    dgvListings.Columns["ListingId"]!.FillWeight = 6;
                }
                if (dgvListings.Columns["ProduceName"] != null)
                {
                    dgvListings.Columns["ProduceName"]!.HeaderText = "Produce / Crop";
                    dgvListings.Columns["ProduceName"]!.FillWeight = 24;
                }
                if (dgvListings.Columns["Category"] != null)
                {
                    dgvListings.Columns["Category"]!.HeaderText = "Category";
                    dgvListings.Columns["Category"]!.FillWeight = 14;
                }
                if (dgvListings.Columns["Quantity"] != null)
                {
                    dgvListings.Columns["Quantity"]!.HeaderText = "Qty (kg)";
                    dgvListings.Columns["Quantity"]!.FillWeight = 12;
                    dgvListings.Columns["Quantity"]!.DefaultCellStyle.Format = "N0";
                }
                if (dgvListings.Columns["PricePerUnit"] != null)
                {
                    dgvListings.Columns["PricePerUnit"]!.HeaderText = "Price/kg (RWF)";
                    dgvListings.Columns["PricePerUnit"]!.FillWeight = 14;
                    dgvListings.Columns["PricePerUnit"]!.DefaultCellStyle.Format = "N0";
                }
                if (dgvListings.Columns["District"] != null)
                {
                    dgvListings.Columns["District"]!.HeaderText = "District";
                    dgvListings.Columns["District"]!.FillWeight = 12;
                }
                if (dgvListings.Columns["FarmerName"] != null)
                {
                    dgvListings.Columns["FarmerName"]!.HeaderText = "Farmer";
                    dgvListings.Columns["FarmerName"]!.FillWeight = 18;
                }

                // Hide technical / non-grid properties
                string[] hidden = { "FarmerId", "OriginalQuantity", "Unit", "PickupLocation", "Description", "ImageUrl", "CreatedAt", "IsAvailable" };
                foreach (var h in hidden)
                {
                    if (dgvListings.Columns[h] != null)
                        dgvListings.Columns[h]!.Visible = false;
                }
            }
        }

        private void ShowError(string msg)
        {
            lblStatus.ForeColor = Color.OrangeRed;
            lblStatus.Text = $"⚠  {msg}";
        }
    }
}
