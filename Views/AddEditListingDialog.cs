using AgriConnect.Controllers;
using AgriConnect.Models;

namespace AgriConnect.Views
{
    public class AddEditListingDialog : Form
    {
        private readonly FarmerController _controller = new();
        private readonly Listing? _existingListing;
        public Listing? ResultListing { get; private set; }

        private TextBox txtProduceName = null!;
        private ComboBox cmbCategory = null!;
        private NumericUpDown numQuantity = null!;
        private ComboBox cmbUnit = null!;
        private NumericUpDown numPrice = null!;
        private DateTimePicker dtpHarvest = null!;
        private TextBox txtPickupLocation = null!;
        private ComboBox cmbDistrict = null!;
        private TextBox txtDescription = null!;
        private Label lblSuggestedPrice = null!;

        public AddEditListingDialog(Listing? existing = null)
        {
            _existingListing = existing;
            InitializeComponent();
            if (_existingListing != null)
            {
                PopulateFields(_existingListing);
            }
            else
            {
                UpdateSuggestedPrice();
            }
        }

        private void InitializeComponent()
        {
            Text = _existingListing == null ? "Create New Produce Listing - AgriConnect" : "Edit Produce Listing - AgriConnect";
            Size = new Size(580, 640);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9.5f);

            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(46, 125, 50)
            };
            var lblTitle = new Label
            {
                Text = _existingListing == null ? "🌱 New Farm Produce Listing" : "✏️ Edit Produce Listing",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 12),
                AutoSize = true
            };
            var lblSub = new Label
            {
                Text = "Connect directly with Rwandan buyers without middlemen",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(232, 245, 233),
                Location = new Point(22, 38),
                AutoSize = true
            };
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSub });

            var pnlBody = new Panel
            {
                Location = new Point(20, 75),
                Size = new Size(520, 480),
                AutoScroll = true
            };

            int y = 10;
            void AddRow(string labelText, Control ctrl, int height = 28)
            {
                var lbl = new Label
                {
                    Text = labelText,
                    Location = new Point(10, y),
                    Size = new Size(160, 24),
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 50, 50)
                };
                ctrl.Location = new Point(180, y);
                ctrl.Size = new Size(320, height);
                pnlBody.Controls.Add(lbl);
                pnlBody.Controls.Add(ctrl);
                y += height + 12;
            }

            txtProduceName = new TextBox();
            txtProduceName.TextChanged += (s, e) => UpdateSuggestedPrice();
            AddRow("Produce Name *", txtProduceName);

            cmbCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.Items.AddRange(new object[] { "Tubers", "Cereals", "Vegetables", "Legumes", "Fruits", "Coffee & Tea" });
            cmbCategory.SelectedIndex = 0;
            AddRow("Category *", cmbCategory);

            numQuantity = new NumericUpDown { Minimum = 1, Maximum = 100000, Value = 100 };
            AddRow("Available Quantity *", numQuantity);

            cmbUnit = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbUnit.Items.AddRange(new object[] { "kg", "Sack (50kg)", "Crate", "Bunch" });
            cmbUnit.SelectedIndex = 0;
            AddRow("Unit of Measure *", cmbUnit);

            numPrice = new NumericUpDown { Minimum = 50, Maximum = 1000000, Value = 500, Increment = 50 };
            AddRow("Price per Unit (RWF) *", numPrice);

            lblSuggestedPrice = new Label
            {
                Location = new Point(180, y - 6),
                Size = new Size(320, 20),
                Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                ForeColor = Color.FromArgb(46, 125, 50),
                Text = "💡 Suggested fair market price: RWF 450 - 550 / kg"
            };
            pnlBody.Controls.Add(lblSuggestedPrice);
            y += 24;

            dtpHarvest = new DateTimePicker { Value = DateTime.Today };
            AddRow("Harvest Date *", dtpHarvest);

            cmbDistrict = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDistrict.Items.AddRange(new object[] { "Musanze", "Nyagatare", "Rubavu", "Huye", "Gasabo", "Kicukiro", "Nyarugenge", "Bugesera", "Rwamagana" });
            cmbDistrict.SelectedIndex = 0;
            cmbDistrict.SelectedIndexChanged += (s, e) => UpdateSuggestedPrice();
            AddRow("District *", cmbDistrict);

            txtPickupLocation = new TextBox { Text = "Local Farm / Sector Center" };
            AddRow("Pickup Location *", txtPickupLocation);

            txtDescription = new TextBox { Multiline = true, Height = 60 };
            AddRow("Description & Quality", txtDescription, 60);

            var btnSave = new Button
            {
                Text = _existingListing == null ? "Publish Listing" : "Update Listing",
                BackColor = Color.FromArgb(46, 125, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Location = new Point(240, 560),
                Size = new Size(150, 36)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            var btnCancel = new Button
            {
                Text = "Cancel",
                BackColor = Color.FromArgb(230, 230, 230),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(400, 560),
                Size = new Size(100, 36)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] { pnlHeader, pnlBody, btnSave, btnCancel });
        }

        private void PopulateFields(Listing l)
        {
            txtProduceName.Text = l.ProduceName;
            cmbCategory.SelectedItem = l.Category;
            numQuantity.Value = l.Quantity;
            cmbUnit.SelectedItem = l.Unit;
            numPrice.Value = l.PricePerUnit;
            dtpHarvest.Value = l.HarvestDate;
            cmbDistrict.SelectedItem = l.District;
            txtPickupLocation.Text = l.PickupLocation;
            txtDescription.Text = l.Description;
        }

        private void UpdateSuggestedPrice()
        {
            string crop = string.IsNullOrWhiteSpace(txtProduceName.Text) ? "Produce" : txtProduceName.Text;
            string district = cmbDistrict.SelectedItem?.ToString() ?? "Musanze";
            var (min, max, avg) = _controller.SuggestFairPrice(crop, district);
            lblSuggestedPrice.Text = $"💡 Market Suggestion: RWF {min:N0} - {max:N0} (Avg ~{avg:N0})";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProduceName.Text))
            {
                MessageBox.Show("Please enter the produce name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var listing = _existingListing ?? new Listing();
            listing.FarmerId = AppSession.CurrentUser?.UserId ?? 2;
            listing.FarmerName = AppSession.CurrentUser?.FullName ?? "Jean Claude Hakizimana";
            listing.ProduceName = txtProduceName.Text.Trim();
            listing.Category = cmbCategory.SelectedItem?.ToString() ?? "Vegetables";
            listing.Quantity = numQuantity.Value;
            listing.Unit = cmbUnit.SelectedItem?.ToString() ?? "kg";
            listing.PricePerUnit = numPrice.Value;
            listing.HarvestDate = dtpHarvest.Value;
            listing.District = cmbDistrict.SelectedItem?.ToString() ?? "Musanze";
            listing.PickupLocation = txtPickupLocation.Text.Trim();
            listing.Description = txtDescription.Text.Trim();
            listing.Status = ListingStatus.Active;

            ResultListing = listing;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
