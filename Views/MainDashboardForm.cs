using System.Data;
using System.Text.Json;
using AgriConnect.Controllers;
using AgriConnect.Helpers;
using AgriConnect.Models;
using AgriConnect.Repositories;

namespace AgriConnect.Views
{
    public class MainDashboardForm : Form
    {
        // Controllers
        private readonly FarmerController _farmerCtrl = new();
        private readonly BuyerController _buyerCtrl = new();
        private readonly TransporterController _transporterCtrl = new();
        private readonly MarketController _marketCtrl = new();
        private readonly AdvisoryController _advisoryCtrl = new();
        private readonly AdminController _adminCtrl = new();
        private readonly AppDatabase _db = AppDatabase.Instance;

        // UI Header Controls
        private Panel pnlHeader = null!;
        private Label lblBrand = null!;
        private Label lblUserBadge = null!;
        private ComboBox cmbLanguage = null!;
        private Button btnSwitchUser = null!;
        private Button btnLogout = null!;

        // Left Navigation
        private Panel pnlSidebar = null!;
        private List<Button> _navButtons = new();

        // Main Content Area
        private Panel pnlContent = null!;

        // Views / Tabs
        private Panel pnlOverview = null!;
        private Panel pnlFarmer = null!;
        private Panel pnlBuyer = null!;
        private Panel pnlOrders = null!;
        private Panel pnlMarket = null!;
        private Panel pnlAdvisory = null!;
        private Panel pnlTransport = null!;
        private Panel pnlAdmin = null!;
        private Panel pnlDatabase = null!;

        public MainDashboardForm()
        {
            InitializeComponent();
            _db.OnDatabaseChanged += () =>
            {
                if (!IsDisposed && IsHandleCreated)
                {
                    BeginInvoke(new Action(RefreshCurrentView));
                }
            };
            SwitchToTab("Overview");
            UpdateUserBadge();
        }

        private void InitializeComponent()
        {
            Text = "AgriConnect Rwanda - Smart Farm Produce Marketplace & Advisory Platform (MVC)";
            Size = new Size(1280, 800);
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = AppTheme.OffWhite;
            Font = new Font("Segoe UI", 9.5f);

            BuildHeader();
            BuildSidebar();
            BuildContentPanels();

            Controls.Add(pnlContent);
            Controls.Add(pnlSidebar);
            Controls.Add(pnlHeader);
        }

        private void BuildHeader()
        {
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = AppTheme.Charcoal
            };

            lblBrand = new Label
            {
                Text = "🌱 AgriConnect Rwanda",
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = AppTheme.White,
                Location = new Point(18, 16),
                AutoSize = true
            };

            lblUserBadge = new Label
            {
                Text = "👤 Loading...",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = AppTheme.OffWhite,
                Location = new Point(340, 22),
                AutoSize = true
            };

            cmbLanguage = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(880, 19),
                Size = new Size(110, 26),
                Font = new Font("Segoe UI", 9f),
                BackColor = AppTheme.DarkGrey,
                ForeColor = AppTheme.White
            };
            cmbLanguage.Items.AddRange(new object[] { "English", "Kinyarwanda", "Français" });
            cmbLanguage.SelectedItem = AppSession.CurrentLanguage;
            cmbLanguage.SelectedIndexChanged += (s, e) =>
            {
                AppSession.CurrentLanguage = cmbLanguage.SelectedItem?.ToString() ?? "English";
                UpdateLanguage();
            };

            btnSwitchUser = new Button
            {
                Text = "⚡ Switch Role",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location = new Point(1005, 17),
                Size = new Size(115, 30)
            };
            AppTheme.StyleMutedButton(btnSwitchUser);
            btnSwitchUser.Click += BtnSwitchUser_Click;

            btnLogout = new Button
            {
                Text = "Logout",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location = new Point(1130, 17),
                Size = new Size(80, 30)
            };
            AppTheme.StyleSecondaryButton(btnLogout);
            btnLogout.Click += (s, e) =>
            {
                AppSession.Logout();
                Close();
            };

            pnlHeader.Controls.AddRange(new Control[] { lblBrand, lblUserBadge, cmbLanguage, btnSwitchUser, btnLogout });
        }

        private void BuildSidebar()
        {
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = AppTheme.DeepCharcoal
            };

            string[] tabs = {
                "🏠 Overview",
                "🌾 Farmer Produce",
                "🛒 Marketplace",
                "📦 Orders & Escrow",
                "📈 Market Prices",
                "📚 Advisory & RAB",
                "🚚 Logistics",
                "🛡️ Admin Console",
                "🗄️ Database GUI"
            };

            foreach (var tab in tabs)
            {
                var btn = new Button
                {
                    Text = tab,
                    Tag = tab.Split(' ')[1],
                    Dock = DockStyle.Top,
                    Height = 46,
                    BackColor = Color.Transparent,
                    ForeColor = AppTheme.SlateBorder,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(18, 0, 0, 0),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = AppTheme.DarkGrey;
                btn.Click += (s, e) => SwitchToTab(btn.Tag?.ToString() ?? "Overview");
                _navButtons.Add(btn);
            }

            // Add from bottom to top for DockStyle.Top
            for (int i = _navButtons.Count - 1; i >= 0; i--)
            {
                pnlSidebar.Controls.Add(_navButtons[i]);
            }
        }

        private void BuildContentPanels()
        {
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.OffWhite,
                Padding = new Padding(15)
            };

            pnlOverview = CreateOverviewPanel();
            pnlFarmer = CreateFarmerPanel();
            pnlBuyer = CreateBuyerPanel();
            pnlOrders = CreateOrdersPanel();
            pnlMarket = CreateMarketPanel();
            pnlAdvisory = CreateAdvisoryPanel();
            pnlTransport = CreateTransportPanel();
            pnlAdmin = CreateAdminPanel();
            pnlDatabase = CreateDatabasePanel();

            pnlContent.Controls.AddRange(new Control[] {
                pnlOverview, pnlFarmer, pnlBuyer, pnlOrders, pnlMarket,
                pnlAdvisory, pnlTransport, pnlAdmin, pnlDatabase
            });
        }

        private void SwitchToTab(string tabName)
        {
            foreach (var btn in _navButtons)
            {
                bool active = btn.Tag?.ToString() == tabName;
                btn.BackColor = active ? AppTheme.DarkGrey : Color.Transparent;
                btn.ForeColor = active ? AppTheme.White : AppTheme.SlateBorder;
            }

            pnlOverview.Visible = tabName == "Overview";
            pnlFarmer.Visible = tabName == "Farmer";
            pnlBuyer.Visible = tabName == "Marketplace";
            pnlOrders.Visible = tabName == "Orders";
            pnlMarket.Visible = tabName == "Market";
            pnlAdvisory.Visible = tabName == "Advisory";
            pnlTransport.Visible = tabName == "Logistics";
            pnlAdmin.Visible = tabName == "Admin";
            pnlDatabase.Visible = tabName == "Database";

            RefreshCurrentView();
        }

        private void UpdateUserBadge()
        {
            var u = AppSession.CurrentUser;
            if (u != null)
            {
                lblUserBadge.Text = $"👤 {u.FullName} | Role: [{u.Role}] | Location: {u.District} | Wallet: RWF {u.WalletBalance:N0}";
            }
            else
            {
                lblUserBadge.Text = "Not Logged In";
            }
        }

        private void BtnSwitchUser_Click(object? sender, EventArgs e)
        {
            var menu = new ContextMenuStrip();
            var users = _db.Data.Users;
            foreach (var user in users)
            {
                var item = menu.Items.Add($"[{user.Role}] {user.FullName} ({user.District})");
                item.Click += (s, args) =>
                {
                    AppSession.CurrentUser = user;
                    UpdateUserBadge();
                    RefreshCurrentView();
                    MessageBox.Show($"Switched active session to: {user.FullName} ({user.Role})", "Role Switched", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
            }
            menu.Show(btnSwitchUser, new Point(0, btnSwitchUser.Height));
        }

        private void UpdateLanguage()
        {
            string lang = AppSession.CurrentLanguage;
            if (lang == "Kinyarwanda")
            {
                lblBrand.Text = "🌱 AgriConnect Rwanda";
                btnSwitchUser.Text = "⚡ Hindura Umukoresha";
                btnLogout.Text = "Sohoka";
            }
            else if (lang == "Français")
            {
                lblBrand.Text = "🌱 AgriConnect Rwanda";
                btnSwitchUser.Text = "⚡ Changer de Rôle";
                btnLogout.Text = "Déconnexion";
            }
            else
            {
                lblBrand.Text = "🌱 AgriConnect Rwanda";
                btnSwitchUser.Text = "⚡ Switch Role";
                btnLogout.Text = "Logout";
            }
            RefreshCurrentView();
        }

        private void RefreshCurrentView()
        {
            UpdateUserBadge();
            if (pnlOverview.Visible) RefreshOverview();
            if (pnlFarmer.Visible) RefreshFarmer();
            if (pnlBuyer.Visible) RefreshBuyer();
            if (pnlOrders.Visible) RefreshOrders();
            if (pnlMarket.Visible) RefreshMarket();
            if (pnlAdvisory.Visible) RefreshAdvisory();
            if (pnlTransport.Visible) RefreshTransport();
            if (pnlAdmin.Visible) RefreshAdmin();
            if (pnlDatabase.Visible) RefreshDatabase();
        }

        // ==========================================
        // 1. OVERVIEW PANEL
        // ==========================================
        private Label lblStatListings = null!;
        private Label lblStatOrders = null!;
        private Label lblStatVolume = null!;
        private Label lblStatEscrow = null!;
        private Label lblWeather = null!;

        private Panel CreateOverviewPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.OffWhite };

            var lblHeading = new Label
            {
                Text = "📊 AgriConnect Rwanda Dashboard & System Overview",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(15, 10),
                AutoSize = true
            };
            pnl.Controls.Add(lblHeading);

            // KPI Cards row
            var flpCards = new FlowLayoutPanel
            {
                Location = new Point(10, 45),
                Size = new Size(1020, 110),
                FlowDirection = FlowDirection.LeftToRight
            };

            Panel CreateCard(string title, out Label valLabel)
            {
                var card = new Panel
                {
                    Size = new Size(240, 95),
                    BackColor = AppTheme.White,
                    Margin = new Padding(6),
                    Padding = new Padding(12),
                    BorderStyle = BorderStyle.FixedSingle
                };
                var lTitle = new Label
                {
                    Text = title,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = AppTheme.TextSecondary,
                    Dock = DockStyle.Top,
                    Height = 22
                };
                valLabel = new Label
                {
                    Text = "...",
                    Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                    ForeColor = AppTheme.TextPrimary,
                    Dock = DockStyle.Fill
                };
                card.Controls.Add(valLabel);
                card.Controls.Add(lTitle);
                return card;
            }

            flpCards.Controls.Add(CreateCard("Active Produce Listings", out lblStatListings));
            flpCards.Controls.Add(CreateCard("Total Marketplace Orders", out lblStatOrders));
            flpCards.Controls.Add(CreateCard("Total Trade Volume", out lblStatVolume));
            flpCards.Controls.Add(CreateCard("Held in Escrow Protection", out lblStatEscrow));
            pnl.Controls.Add(flpCards);

            // Weather & Climate Alert Card
            var pnlWeatherCard = new Panel
            {
                Location = new Point(15, 170),
                Size = new Size(1005, 100),
                BackColor = AppTheme.LightGrey,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(12)
            };
            var lblWTitle = new Label
            {
                Text = "🌦️ Rwanda Meteorological Agency (Meteo Rwanda) Advisory & Weather Feed",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = AppTheme.Charcoal,
                Dock = DockStyle.Top,
                Height = 24
            };
            lblWeather = new Label
            {
                Text = "Musanze & Northern Province: High rainfall expected in volcanic zone. Protect Irish potato ridges against waterlogging.\nEastern Province (Nyagatare): Moderate temperatures, favorable harvesting weather for maize & beans.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = AppTheme.TextPrimary,
                Dock = DockStyle.Fill
            };
            pnlWeatherCard.Controls.Add(lblWeather);
            pnlWeatherCard.Controls.Add(lblWTitle);
            pnl.Controls.Add(pnlWeatherCard);

            // Quick Shortcut buttons
            var pnlShortcuts = new GroupBox
            {
                Text = "Quick Role Shortcuts",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                BackColor = AppTheme.White,
                Location = new Point(15, 290),
                Size = new Size(1005, 110)
            };

            Button MakeActionBtn(string text, Action act, int x, bool primary = false)
            {
                var b = new Button
                {
                    Text = text,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Location = new Point(x, 35),
                    Size = new Size(225, 45)
                };
                if (primary) AppTheme.StylePrimaryButton(b);
                else AppTheme.StyleSecondaryButton(b);
                b.Click += (s, e) => act();
                return b;
            }

            pnlShortcuts.Controls.Add(MakeActionBtn("🌾 Manage My Crops", () => SwitchToTab("Farmer"), 20, true));
            pnlShortcuts.Controls.Add(MakeActionBtn("🛒 Browse & Buy Produce", () => SwitchToTab("Marketplace"), 265));
            pnlShortcuts.Controls.Add(MakeActionBtn("📦 Orders & Escrow Tracking", () => SwitchToTab("Orders"), 510));
            pnlShortcuts.Controls.Add(MakeActionBtn("🗄️ Inspect Database GUI", () => SwitchToTab("Database"), 755));
            pnl.Controls.Add(pnlShortcuts);

            // System highlights summary
            var lblInfo = new Label
            {
                Text = "📌 AgriConnect Highlights (IEEE 830 Specification):\n" +
                       "• MVC Architecture: Controllers mediate business logic between WinForms Views and persistent Models.\n" +
                       "• Direct GUI Database Interaction: Browse, query, and mutate all database tables with real-time persistence.\n" +
                       "• Escrow Payment Security: Funds held in escrow via MTN MoMo / Airtel Money simulation until buyer confirms delivery.\n" +
                       "• Professional Design: Styled in an enterprise grey and white monochrome aesthetic with high legibility.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(15, 420),
                Size = new Size(1005, 120)
            };
            pnl.Controls.Add(lblInfo);

            return pnl;
        }

        private void RefreshOverview()
        {
            var (users, listings, orders, volume, escrow) = _adminCtrl.GetPlatformStats();
            lblStatListings.Text = listings.ToString();
            lblStatOrders.Text = orders.ToString();
            lblStatVolume.Text = $"RWF {volume:N0}";
            lblStatEscrow.Text = $"RWF {escrow:N0}";
        }

        // ==========================================
        // 2. FARMER PRODUCE PANEL
        // ==========================================
        private DataGridView dgvFarmerListings = null!;
        private DataGridView dgvFarmerOrders = null!;
        private Label lblFarmerEarnings = null!;

        private Panel CreateFarmerPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.OffWhite };

            var lblTitle = new Label
            {
                Text = "🌾 Farmer Produce & Order Management Center",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(15, 10),
                AutoSize = true
            };
            pnl.Controls.Add(lblTitle);

            // Earnings Card
            lblFarmerEarnings = new Label
            {
                Text = "Earnings Summary: Today: RWF 0 | This Week: RWF 0 | This Month: RWF 0 | Total: RWF 0",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = AppTheme.Charcoal,
                Location = new Point(15, 40),
                Size = new Size(1000, 25)
            };
            pnl.Controls.Add(lblFarmerEarnings);

            // Action toolbar
            var pnlTools = new FlowLayoutPanel
            {
                Location = new Point(15, 70),
                Size = new Size(1005, 40),
                FlowDirection = FlowDirection.LeftToRight
            };

            Button MakeBtn(string text, Action act, bool primary = false)
            {
                var b = new Button
                {
                    Text = text,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Size = new Size(150, 32),
                    Margin = new Padding(0, 0, 10, 0)
                };
                if (primary) AppTheme.StylePrimaryButton(b);
                else AppTheme.StyleSecondaryButton(b);
                b.Click += (s, e) => act();
                return b;
            }

            pnlTools.Controls.Add(MakeBtn("➕ List New Produce", () =>
            {
                using var dlg = new AddEditListingDialog();
                if (dlg.ShowDialog() == DialogResult.OK && dlg.ResultListing != null)
                {
                    var (succ, msg) = _farmerCtrl.CreateListing(dlg.ResultListing);
                    MessageBox.Show(msg, "Produce Listing", MessageBoxButtons.OK, succ ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                    RefreshFarmer();
                }
            }, true));

            pnlTools.Controls.Add(MakeBtn("✏️ Edit Selected", () =>
            {
                var sel = GetSelectedFarmerListing();
                if (sel == null) return;
                using var dlg = new AddEditListingDialog(sel);
                if (dlg.ShowDialog() == DialogResult.OK && dlg.ResultListing != null)
                {
                    var (succ, msg) = _farmerCtrl.UpdateListing(dlg.ResultListing);
                    MessageBox.Show(msg, "Produce Listing", MessageBoxButtons.OK, succ ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                    RefreshFarmer();
                }
            }));

            pnlTools.Controls.Add(MakeBtn("⏯️ Pause / Activate", () =>
            {
                var sel = GetSelectedFarmerListing();
                if (sel == null) return;
                var (succ, msg) = _farmerCtrl.TogglePauseListing(sel.ListingId);
                MessageBox.Show(msg, "Listing Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshFarmer();
            }));

            pnlTools.Controls.Add(MakeBtn("🗑️ Remove Listing", () =>
            {
                var sel = GetSelectedFarmerListing();
                if (sel == null) return;
                if (MessageBox.Show($"Delete listing #{sel.ListingId} ({sel.ProduceName})?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _farmerCtrl.DeleteListing(sel.ListingId);
                    RefreshFarmer();
                }
            }));

            pnl.Controls.Add(pnlTools);

            // Listings Grid
            var lblGridTitle = new Label { Text = "My Active & Paused Listings:", Location = new Point(15, 115), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
            pnl.Controls.Add(lblGridTitle);

            dgvFarmerListings = new DataGridView
            {
                Location = new Point(15, 140),
                Size = new Size(1005, 200),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvFarmerListings);
            pnl.Controls.Add(dgvFarmerListings);

            // Incoming Orders Section
            var lblOrdersTitle = new Label { Text = "Incoming Customer Orders (Accept / Dispatch):", Location = new Point(15, 355), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
            pnl.Controls.Add(lblOrdersTitle);

            var pnlOrderTools = new FlowLayoutPanel
            {
                Location = new Point(15, 380),
                Size = new Size(1005, 36),
                FlowDirection = FlowDirection.LeftToRight
            };

            pnlOrderTools.Controls.Add(MakeBtn("✅ Accept Order", () =>
            {
                var ord = GetSelectedFarmerOrder();
                if (ord == null) return;
                var (succ, msg) = _farmerCtrl.AcceptOrder(ord.OrderId);
                MessageBox.Show(msg, "Order Status", MessageBoxButtons.OK, succ ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                RefreshFarmer();
            }, true));

            pnlOrderTools.Controls.Add(MakeBtn("📦 Mark Produce Ready", () =>
            {
                var ord = GetSelectedFarmerOrder();
                if (ord == null) return;
                var (succ, msg) = _farmerCtrl.MarkOrderReady(ord.OrderId);
                MessageBox.Show(msg, "Order Status", MessageBoxButtons.OK, succ ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                RefreshFarmer();
            }));

            pnlOrderTools.Controls.Add(MakeBtn("❌ Reject Order", () =>
            {
                var ord = GetSelectedFarmerOrder();
                if (ord == null) return;
                var (succ, msg) = _farmerCtrl.RejectOrder(ord.OrderId);
                MessageBox.Show(msg, "Order Status", MessageBoxButtons.OK, succ ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                RefreshFarmer();
            }));

            pnl.Controls.Add(pnlOrderTools);

            dgvFarmerOrders = new DataGridView
            {
                Location = new Point(15, 420),
                Size = new Size(1005, 230),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvFarmerOrders);
            pnl.Controls.Add(dgvFarmerOrders);

            return pnl;
        }

        private Listing? GetSelectedFarmerListing()
        {
            if (dgvFarmerListings.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a listing from the table first.", "Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            int id = Convert.ToInt32(dgvFarmerListings.SelectedRows[0].Cells["ListingId"].Value);
            return _db.Data.Listings.FirstOrDefault(l => l.ListingId == id);
        }

        private Order? GetSelectedFarmerOrder()
        {
            if (dgvFarmerOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an order from the table first.", "Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            int id = Convert.ToInt32(dgvFarmerOrders.SelectedRows[0].Cells["OrderId"].Value);
            return _db.Data.Orders.FirstOrDefault(o => o.OrderId == id);
        }

        private void RefreshFarmer()
        {
            int farmerId = AppSession.CurrentUser?.Role == UserRole.Farmer ? AppSession.CurrentUser.UserId : 2;
            var listings = _farmerCtrl.GetMyListings(farmerId);
            dgvFarmerListings.DataSource = listings.Select(l => new
            {
                l.ListingId,
                l.ProduceName,
                l.Category,
                Quantity = $"{l.Quantity} {l.Unit}",
                Price = $"RWF {l.PricePerUnit:N0}",
                l.District,
                l.PickupLocation,
                l.Status,
                HarvestDate = l.HarvestDate.ToString("yyyy-MM-dd")
            }).ToList();

            var orders = _farmerCtrl.GetIncomingOrders(farmerId);
            dgvFarmerOrders.DataSource = orders.Select(o => new
            {
                o.OrderId,
                o.BuyerName,
                o.BuyerPhone,
                ProduceTotal = $"RWF {o.ProduceTotal:N0}",
                o.DeliveryType,
                o.Status,
                Date = o.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            }).ToList();

            var (today, week, month, total) = _farmerCtrl.GetEarningsSummary(farmerId);
            lblFarmerEarnings.Text = $"💰 Earnings Summary (After 3% platform fee): Today: RWF {today:N0} | This Week: RWF {week:N0} | This Month: RWF {month:N0} | All-Time: RWF {total:N0}";
        }

        // ==========================================
        // 3. BUYER MARKETPLACE PANEL
        // ==========================================
        private TextBox txtSearchMarket = null!;
        private ComboBox cmbCatFilter = null!;
        private ComboBox cmbDistrictFilter = null!;
        private ComboBox cmbSortFilter = null!;
        private DataGridView dgvMarketplace = null!;
        private Label lblCartSummary = null!;
        private NumericUpDown numBuyQty = null!;

        private Panel CreateBuyerPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.OffWhite };

            var lblTitle = new Label
            {
                Text = "🛒 Rwanda Produce Marketplace & Smart Shopping",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(15, 10),
                AutoSize = true
            };
            pnl.Controls.Add(lblTitle);

            // Filter Bar
            var pnlFilters = new Panel
            {
                Location = new Point(15, 45),
                Size = new Size(1005, 45),
                BackColor = AppTheme.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblSearch = new Label { Text = "Search:", Location = new Point(10, 12), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
            txtSearchMarket = new TextBox { Location = new Point(65, 9), Size = new Size(180, 24), BackColor = AppTheme.OffWhite, ForeColor = AppTheme.TextPrimary };
            txtSearchMarket.TextChanged += (s, e) => RefreshBuyer();

            var lblCat = new Label { Text = "Category:", Location = new Point(260, 12), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
            cmbCatFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(330, 9), Size = new Size(130, 24), BackColor = AppTheme.OffWhite, ForeColor = AppTheme.TextPrimary };
            cmbCatFilter.Items.AddRange(new object[] { "All Categories", "Tubers", "Cereals", "Vegetables", "Legumes", "Fruits", "Coffee & Tea" });
            cmbCatFilter.SelectedIndex = 0;
            cmbCatFilter.SelectedIndexChanged += (s, e) => RefreshBuyer();

            var lblDist = new Label { Text = "District:", Location = new Point(475, 12), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
            cmbDistrictFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(535, 9), Size = new Size(120, 24), BackColor = AppTheme.OffWhite, ForeColor = AppTheme.TextPrimary };
            cmbDistrictFilter.Items.AddRange(new object[] { "All Districts", "Musanze", "Nyagatare", "Rubavu", "Huye", "Gasabo", "Kicukiro" });
            cmbDistrictFilter.SelectedIndex = 0;
            cmbDistrictFilter.SelectedIndexChanged += (s, e) => RefreshBuyer();

            var lblSort = new Label { Text = "Sort:", Location = new Point(670, 12), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
            cmbSortFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(710, 9), Size = new Size(150, 24), BackColor = AppTheme.OffWhite, ForeColor = AppTheme.TextPrimary };
            cmbSortFilter.Items.AddRange(new object[] { "Newest Listings", "Price: Low to High", "Price: High to Low", "Quantity Available" });
            cmbSortFilter.SelectedIndex = 0;
            cmbSortFilter.SelectedIndexChanged += (s, e) => RefreshBuyer();

            var btnResetFilters = new Button
            {
                Text = "Reset",
                Location = new Point(880, 8),
                Size = new Size(70, 26)
            };
            AppTheme.StyleSecondaryButton(btnResetFilters);
            btnResetFilters.Click += (s, e) =>
            {
                txtSearchMarket.Clear();
                cmbCatFilter.SelectedIndex = 0;
                cmbDistrictFilter.SelectedIndex = 0;
                cmbSortFilter.SelectedIndex = 0;
                RefreshBuyer();
            };

            pnlFilters.Controls.AddRange(new Control[] {
                lblSearch, txtSearchMarket, lblCat, cmbCatFilter,
                lblDist, cmbDistrictFilter, lblSort, cmbSortFilter, btnResetFilters
            });
            pnl.Controls.Add(pnlFilters);

            // Marketplace Grid
            dgvMarketplace = new DataGridView
            {
                Location = new Point(15, 100),
                Size = new Size(1005, 350),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvMarketplace);
            pnl.Controls.Add(dgvMarketplace);

            // Purchase / Cart Bar
            var pnlCartActions = new Panel
            {
                Location = new Point(15, 465),
                Size = new Size(1005, 75),
                BackColor = AppTheme.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            var lblQty = new Label { Text = "Quantity to Order:", Location = new Point(15, 25), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
            numBuyQty = new NumericUpDown { Location = new Point(150, 23), Size = new Size(100, 26), Minimum = 1, Maximum = 10000, Value = 10, BackColor = AppTheme.OffWhite, ForeColor = AppTheme.TextPrimary };

            var btnAddToCart = new Button
            {
                Text = "🛒 Add to Cart",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(270, 20),
                Size = new Size(140, 34)
            };
            AppTheme.StyleSecondaryButton(btnAddToCart);
            btnAddToCart.Click += (s, e) =>
            {
                if (dgvMarketplace.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a produce item from the marketplace table.", "Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                int id = Convert.ToInt32(dgvMarketplace.SelectedRows[0].Cells["ListingId"].Value);
                var listing = _db.Data.Listings.FirstOrDefault(l => l.ListingId == id);
                if (listing != null)
                {
                    var (succ, msg) = _buyerCtrl.AddToCart(listing, numBuyQty.Value);
                    MessageBox.Show(msg, "Shopping Cart", MessageBoxButtons.OK, succ ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                    UpdateCartDisplay();
                }
            };

            lblCartSummary = new Label
            {
                Text = "Cart: 0 items (RWF 0)",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppTheme.Charcoal,
                Location = new Point(440, 25),
                Size = new Size(280, 26)
            };

            var btnViewCheckout = new Button
            {
                Text = "🔒 Checkout & Escrow Pay",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Location = new Point(730, 18),
                Size = new Size(200, 38)
            };
            AppTheme.StylePrimaryButton(btnViewCheckout);
            btnViewCheckout.Click += (s, e) =>
            {
                if (!AppSession.Cart.Any())
                {
                    MessageBox.Show("Your cart is empty. Add produce items first.", "Cart Empty", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                using var dlg = new CheckoutDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    UpdateCartDisplay();
                    RefreshBuyer();
                    SwitchToTab("Orders");
                }
            };

            var btnClearCart = new Button
            {
                Text = "Clear",
                Location = new Point(940, 23),
                Size = new Size(50, 28)
            };
            AppTheme.StyleSecondaryButton(btnClearCart);
            btnClearCart.Click += (s, e) =>
            {
                _buyerCtrl.ClearCart();
                UpdateCartDisplay();
            };

            pnlCartActions.Controls.AddRange(new Control[] {
                lblQty, numBuyQty, btnAddToCart, lblCartSummary, btnViewCheckout, btnClearCart
            });
            pnl.Controls.Add(pnlCartActions);

            return pnl;
        }

        private void UpdateCartDisplay()
        {
            int count = AppSession.Cart.Count;
            decimal total = AppSession.Cart.Sum(i => i.Subtotal);
            lblCartSummary.Text = $"Cart: {count} item(s) (RWF {total:N0})";
        }

        private void RefreshBuyer()
        {
            string? kw = string.IsNullOrWhiteSpace(txtSearchMarket.Text) ? null : txtSearchMarket.Text.Trim();
            string? cat = cmbCatFilter.SelectedItem?.ToString();
            string? dist = cmbDistrictFilter.SelectedItem?.ToString();
            string? sort = cmbSortFilter.SelectedItem?.ToString();

            var results = _buyerCtrl.SearchProduce(kw, cat, dist, null, sort);
            dgvMarketplace.DataSource = results.Select(r => new
            {
                r.ListingId,
                r.ProduceName,
                r.Category,
                r.FarmerName,
                r.District,
                Available = $"{r.Quantity:N0} {r.Unit}",
                Price = $"RWF {r.PricePerUnit:N0}",
                r.PickupLocation,
                Harvest = r.HarvestDate.ToString("yyyy-MM-dd")
            }).ToList();

            UpdateCartDisplay();
        }

        // ==========================================
        // 4. ORDERS & ESCROW PANEL
        // ==========================================
        private DataGridView dgvOrdersList = null!;

        private Panel CreateOrdersPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.OffWhite };

            var lblTitle = new Label
            {
                Text = "📦 Order History & Escrow Settlement Tracker",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(15, 10),
                AutoSize = true
            };
            pnl.Controls.Add(lblTitle);

            var pnlActions = new FlowLayoutPanel
            {
                Location = new Point(15, 45),
                Size = new Size(1005, 40),
                FlowDirection = FlowDirection.LeftToRight
            };

            Button MakeBtn(string text, Action act, bool primary = false)
            {
                var b = new Button
                {
                    Text = text,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Size = new Size(200, 32),
                    Margin = new Padding(0, 0, 10, 0)
                };
                if (primary) AppTheme.StylePrimaryButton(b);
                else AppTheme.StyleSecondaryButton(b);
                b.Click += (s, e) => act();
                return b;
            }

            pnlActions.Controls.Add(MakeBtn("✅ Confirm Delivery (Release)", () =>
            {
                var ord = GetSelectedOrder();
                if (ord == null) return;

                using var codeForm = new Form
                {
                    Text = "Confirm Delivery Code",
                    Size = new Size(380, 200),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    BackColor = AppTheme.White,
                    Font = new Font("Segoe UI", 9.5f)
                };
                var lblP = new Label { Text = $"Enter Delivery Code for Order #{ord.OrderId}:", Location = new Point(20, 20), AutoSize = true, ForeColor = AppTheme.TextPrimary };
                var txtC = new TextBox { Location = new Point(20, 50), Size = new Size(320, 28), Text = ord.DeliveryCode, BackColor = AppTheme.OffWhite, ForeColor = AppTheme.TextPrimary };
                var btnOk = new Button { Text = "Release Escrow", DialogResult = DialogResult.OK, Location = new Point(180, 100), Size = new Size(130, 34) };
                AppTheme.StylePrimaryButton(btnOk);
                codeForm.Controls.AddRange(new Control[] { lblP, txtC, btnOk });
                codeForm.AcceptButton = btnOk;

                if (codeForm.ShowDialog() == DialogResult.OK)
                {
                    var (succ, msg) = _buyerCtrl.ConfirmDeliveryAndReleaseEscrow(ord.OrderId, txtC.Text.Trim());
                    MessageBox.Show(msg, "Escrow Settlement", MessageBoxButtons.OK, succ ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                    RefreshOrders();
                }
            }, true));

            pnlActions.Controls.Add(MakeBtn("⚠️ Open Dispute", () =>
            {
                var ord = GetSelectedOrder();
                if (ord == null) return;
                using var dlg = new RaiseDisputeDialog(ord.OrderId);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    RefreshOrders();
                }
            }));

            pnlActions.Controls.Add(MakeBtn("❌ Cancel Order", () =>
            {
                var ord = GetSelectedOrder();
                if (ord == null) return;
                var (succ, msg) = _buyerCtrl.CancelOrder(ord.OrderId);
                MessageBox.Show(msg, "Order Cancellation", MessageBoxButtons.OK, succ ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                RefreshOrders();
            }));

            pnl.Controls.Add(pnlActions);

            dgvOrdersList = new DataGridView
            {
                Location = new Point(15, 95),
                Size = new Size(1005, 500),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvOrdersList);
            pnl.Controls.Add(dgvOrdersList);

            return pnl;
        }

        private Order? GetSelectedOrder()
        {
            if (dgvOrdersList.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an order from the list.", "Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            int id = Convert.ToInt32(dgvOrdersList.SelectedRows[0].Cells["OrderId"].Value);
            return _db.Data.Orders.FirstOrDefault(o => o.OrderId == id);
        }

        private void RefreshOrders()
        {
            var user = AppSession.CurrentUser;
            List<Order> orders;
            if (user?.Role == UserRole.Farmer)
                orders = _farmerCtrl.GetIncomingOrders(user.UserId);
            else if (user?.Role == UserRole.Buyer)
                orders = _buyerCtrl.GetMyOrders(user.UserId);
            else
                orders = _db.Data.Orders.OrderByDescending(o => o.CreatedAt).ToList();

            dgvOrdersList.DataSource = orders.Select(o => new
            {
                o.OrderId,
                o.BuyerName,
                o.FarmerName,
                Transporter = o.TransporterName ?? "Not Assigned",
                Total = $"RWF {o.TotalAmount:N0}",
                o.DeliveryType,
                Code = o.DeliveryCode,
                o.Status,
                Date = o.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            }).ToList();
        }

        // ==========================================
        // 5. MARKET PRICES PANEL
        // ==========================================
        private DataGridView dgvMarketPrices = null!;
        private ComboBox cmbPriceDistrict = null!;

        private Panel CreateMarketPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.OffWhite };

            var lblTitle = new Label
            {
                Text = "📈 National Agricultural Market Price Intelligence & Trends",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(15, 10),
                AutoSize = true
            };
            pnl.Controls.Add(lblTitle);

            var pnlToolbar = new Panel
            {
                Location = new Point(15, 45),
                Size = new Size(1005, 40),
                BackColor = AppTheme.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            var lblD = new Label { Text = "Filter by Market / District:", Location = new Point(15, 10), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
            cmbPriceDistrict = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(190, 7), Size = new Size(200, 24), BackColor = AppTheme.OffWhite, ForeColor = AppTheme.TextPrimary };
            cmbPriceDistrict.Items.AddRange(new object[] { "All Rwanda Markets", "Kigali (Kimironko)", "Musanze Central", "Nyagatare Market", "Huye Market", "Rubavu Border" });
            cmbPriceDistrict.SelectedIndex = 0;
            cmbPriceDistrict.SelectedIndexChanged += (s, e) => RefreshMarket();

            pnlToolbar.Controls.AddRange(new Control[] { lblD, cmbPriceDistrict });
            pnl.Controls.Add(pnlToolbar);

            dgvMarketPrices = new DataGridView
            {
                Location = new Point(15, 95),
                Size = new Size(1005, 500),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvMarketPrices);
            pnl.Controls.Add(dgvMarketPrices);

            return pnl;
        }

        private void RefreshMarket()
        {
            var prices = _marketCtrl.GetAllPrices();
            string dist = cmbPriceDistrict.SelectedItem?.ToString() ?? "All Rwanda Markets";
            if (dist != "All Rwanda Markets")
            {
                prices = prices.Where(p => p.MarketName.Contains(dist.Split(' ')[0])).ToList();
            }

            dgvMarketPrices.DataSource = prices.Select(p => new
            {
                p.PriceId,
                p.CropName,
                p.Category,
                p.MarketName,
                CurrentPrice = $"RWF {p.PricePerKg:N0} / {p.Unit}",
                PreviousPrice = $"RWF {p.PreviousPricePerKg:N0} / {p.Unit}",
                Trend = p.Trend,
                Recorded = p.RecordedDate.ToString("yyyy-MM-dd")
            }).ToList();
        }

        // ==========================================
        // 6. ADVISORY & RAB PANEL
        // ==========================================
        private ListBox lstArticles = null!;
        private TextBox txtArticleBody = null!;
        private DataGridView dgvQA = null!;
        private ComboBox cmbAdvisoryLang = null!;

        private Panel CreateAdvisoryPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.OffWhite };

            var lblTitle = new Label
            {
                Text = "📚 Rwanda Agriculture Board (RAB) Advisory & Extension Service",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(15, 10),
                AutoSize = true
            };
            pnl.Controls.Add(lblTitle);

            // Language filter
            var lblLang = new Label { Text = "Article Language:", Location = new Point(15, 45), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
            cmbAdvisoryLang = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(135, 42), Size = new Size(130, 24), BackColor = AppTheme.OffWhite, ForeColor = AppTheme.TextPrimary };
            cmbAdvisoryLang.Items.AddRange(new object[] { "All", "English", "Kinyarwanda", "French" });
            cmbAdvisoryLang.SelectedIndex = 0;
            cmbAdvisoryLang.SelectedIndexChanged += (s, e) => RefreshAdvisory();
            pnl.Controls.AddRange(new Control[] { lblLang, cmbAdvisoryLang });

            // Articles section
            lstArticles = new ListBox
            {
                Location = new Point(15, 75),
                Size = new Size(380, 250),
                Font = new Font("Segoe UI", 9.5f),
                BackColor = AppTheme.White,
                ForeColor = AppTheme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            lstArticles.SelectedIndexChanged += (s, e) =>
            {
                if (lstArticles.SelectedItem is AdvisoryArticle a)
                {
                    txtArticleBody.Text = $"=== {a.Title} ===\nAuthor: {a.AuthorName} | Category: {a.Category} | Lang: {a.Language} | Date: {a.PublishedAt:yyyy-MM-dd}\n\n{a.Content}";
                }
            };
            pnl.Controls.Add(lstArticles);

            txtArticleBody = new TextBox
            {
                Location = new Point(410, 75),
                Size = new Size(610, 250),
                Multiline = true,
                ReadOnly = true,
                BackColor = AppTheme.White,
                ForeColor = AppTheme.TextPrimary,
                Font = new Font("Segoe UI", 10f),
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnl.Controls.Add(txtArticleBody);

            // Q&A Section
            var lblQATitle = new Label
            {
                Text = "Farmer Questions & Agronomist Answers (FR-9.3):",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(15, 335),
                AutoSize = true
            };
            pnl.Controls.Add(lblQATitle);

            var btnAsk = new Button
            {
                Text = "❓ Ask Agronomist a Question",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location = new Point(410, 332),
                Size = new Size(210, 30)
            };
            AppTheme.StylePrimaryButton(btnAsk);
            btnAsk.Click += (s, e) =>
            {
                using var askForm = new Form
                {
                    Text = "Ask an Agronomist",
                    Size = new Size(480, 360),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    BackColor = AppTheme.White,
                    Font = new Font("Segoe UI", 9.5f)
                };
                var l1 = new Label { Text = "Subject *:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
                var tSub = new TextBox { Location = new Point(20, 45), Size = new Size(420, 26), BackColor = AppTheme.OffWhite, ForeColor = AppTheme.TextPrimary };
                var l2 = new Label { Text = "Question / Disease Description *:", Location = new Point(20, 85), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
                var tQ = new TextBox { Location = new Point(20, 110), Size = new Size(420, 130), Multiline = true, BackColor = AppTheme.OffWhite, ForeColor = AppTheme.TextPrimary };
                var bSub = new Button { Text = "Submit Question", DialogResult = DialogResult.OK, Location = new Point(290, 260), Size = new Size(150, 34) };
                AppTheme.StylePrimaryButton(bSub);
                askForm.Controls.AddRange(new Control[] { l1, tSub, l2, tQ, bSub });
                askForm.AcceptButton = bSub;

                if (askForm.ShowDialog() == DialogResult.OK)
                {
                    var q = new FarmerQuestion
                    {
                        FarmerId = AppSession.CurrentUser?.UserId ?? 2,
                        FarmerName = AppSession.CurrentUser?.FullName ?? "Farmer",
                        Subject = tSub.Text.Trim(),
                        QuestionText = tQ.Text.Trim()
                    };
                    var (succ, msg) = _advisoryCtrl.AskQuestion(q);
                    MessageBox.Show(msg, "Agronomy Question", MessageBoxButtons.OK, succ ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                    RefreshAdvisory();
                }
            };
            pnl.Controls.Add(btnAsk);

            var btnAnswer = new Button
            {
                Text = "✍️ Answer Selected Question",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location = new Point(630, 332),
                Size = new Size(200, 30)
            };
            AppTheme.StyleSecondaryButton(btnAnswer);
            btnAnswer.Click += (s, e) =>
            {
                if (dgvQA.SelectedRows.Count == 0) return;
                int qId = Convert.ToInt32(dgvQA.SelectedRows[0].Cells["QuestionId"].Value);
                string ans = SimpleInputDialog.Prompt("Answer Question", "Enter Expert Agronomist Advice / Answer:", "Ensure proper soil aeration and apply copper fungicide preventively.");
                if (!string.IsNullOrWhiteSpace(ans))
                {
                    _advisoryCtrl.AnswerQuestion(qId, ans, AppSession.CurrentUser?.FullName ?? "Dr. Alice Uwase (RAB)");
                    RefreshAdvisory();
                }
            };
            pnl.Controls.Add(btnAnswer);

            dgvQA = new DataGridView
            {
                Location = new Point(15, 370),
                Size = new Size(1005, 230),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvQA);
            pnl.Controls.Add(dgvQA);

            return pnl;
        }

        private void RefreshAdvisory()
        {
            string? lang = cmbAdvisoryLang.SelectedItem?.ToString();
            var articles = _advisoryCtrl.GetArticles(lang);
            lstArticles.DataSource = articles;
            lstArticles.DisplayMember = "Title";

            var questions = _advisoryCtrl.GetQuestions();
            dgvQA.DataSource = questions.Select(q => new
            {
                q.QuestionId,
                q.FarmerName,
                q.Subject,
                q.QuestionText,
                q.AnswerText,
                q.AnsweredBy,
                Status = q.IsAnswered ? "Answered" : "Pending",
                Date = q.AskedAt.ToString("yyyy-MM-dd")
            }).ToList();
        }

        // ==========================================
        // 7. LOGISTICS & TRANSPORT PANEL
        // ==========================================
        private DataGridView dgvTransportJobs = null!;

        private Panel CreateTransportPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.OffWhite };

            var lblTitle = new Label
            {
                Text = "🚚 Rural Transporter Logistics & Dispatch Hub",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(15, 10),
                AutoSize = true
            };
            pnl.Controls.Add(lblTitle);

            var pnlTools = new FlowLayoutPanel
            {
                Location = new Point(15, 45),
                Size = new Size(1005, 40),
                FlowDirection = FlowDirection.LeftToRight
            };

            Button MakeBtn(string text, Action act, bool primary = false)
            {
                var b = new Button
                {
                    Text = text,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Size = new Size(190, 32),
                    Margin = new Padding(0, 0, 10, 0)
                };
                if (primary) AppTheme.StylePrimaryButton(b);
                else AppTheme.StyleSecondaryButton(b);
                b.Click += (s, e) => act();
                return b;
            }

            pnlTools.Controls.Add(MakeBtn("🚚 Claim Delivery Job", () =>
            {
                var job = GetSelectedTransportJob();
                if (job == null) return;
                var u = AppSession.CurrentUser;
                var (succ, msg) = _transporterCtrl.AcceptDeliveryJob(job.OrderId, u?.UserId ?? 5, u?.FullName ?? "Eric Nshimiyimana");
                MessageBox.Show(msg, "Claim Delivery", MessageBoxButtons.OK, succ ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                RefreshTransport();
            }, true));

            pnlTools.Controls.Add(MakeBtn("📦 Confirm Produce Pickup", () =>
            {
                var job = GetSelectedTransportJob();
                if (job == null) return;
                var (succ, msg) = _transporterCtrl.ConfirmPickup(job.OrderId);
                MessageBox.Show(msg, "Pickup Status", MessageBoxButtons.OK, succ ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                RefreshTransport();
            }));

            pnlTools.Controls.Add(MakeBtn("🏁 Confirm Recipient Drop-Off", () =>
            {
                var job = GetSelectedTransportJob();
                if (job == null) return;
                string code = SimpleInputDialog.Prompt("Drop-Off Verification", "Ask the recipient buyer for their 6-character Delivery Code:", job.DeliveryCode);
                if (!string.IsNullOrWhiteSpace(code))
                {
                    var (succ, msg) = _transporterCtrl.ConfirmDropOff(job.OrderId, code);
                    MessageBox.Show(msg, "Drop-off Outcome", MessageBoxButtons.OK, succ ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                    RefreshTransport();
                }
            }));

            pnl.Controls.Add(pnlTools);

            dgvTransportJobs = new DataGridView
            {
                Location = new Point(15, 95),
                Size = new Size(1005, 500),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvTransportJobs);
            pnl.Controls.Add(dgvTransportJobs);

            return pnl;
        }

        private Order? GetSelectedTransportJob()
        {
            if (dgvTransportJobs.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a delivery job first.", "Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            int id = Convert.ToInt32(dgvTransportJobs.SelectedRows[0].Cells["OrderId"].Value);
            return _db.Data.Orders.FirstOrDefault(o => o.OrderId == id);
        }

        private void RefreshTransport()
        {
            var jobs = _db.Data.Orders
                .Where(o => o.DeliveryType == DeliveryType.TransporterDelivery)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            dgvTransportJobs.DataSource = jobs.Select(j => new
            {
                j.OrderId,
                j.BuyerName,
                From = j.PickupAddress,
                To = j.DeliveryAddress,
                DeliveryFee = $"RWF {j.DeliveryFee:N0}",
                AssignedTo = j.TransporterName ?? "Unassigned (Open for Claim)",
                j.Status,
                VerificationCode = j.DeliveryCode
            }).ToList();
        }

        // ==========================================
        // 8. ADMIN CONSOLE PANEL
        // ==========================================
        private DataGridView dgvAdminUsers = null!;
        private DataGridView dgvAdminDisputes = null!;
        private DataGridView dgvAdminLogs = null!;

        private Panel CreateAdminPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.OffWhite };

            var lblTitle = new Label
            {
                Text = "🛡️ AgriConnect Administration, Governance & Compliance",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(15, 10),
                AutoSize = true
            };
            pnl.Controls.Add(lblTitle);

            var pnlExport = new FlowLayoutPanel
            {
                Location = new Point(15, 45),
                Size = new Size(1005, 36),
                FlowDirection = FlowDirection.LeftToRight
            };

            Button MakeBtn(string text, Action act, bool primary = false)
            {
                var b = new Button
                {
                    Text = text,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Size = new Size(180, 30),
                    Margin = new Padding(0, 0, 10, 0)
                };
                if (primary) AppTheme.StylePrimaryButton(b);
                else AppTheme.StyleSecondaryButton(b);
                b.Click += (s, e) => act();
                return b;
            }

            pnlExport.Controls.Add(MakeBtn("📊 Export Orders CSV", () =>
            {
                using var sfd = new SaveFileDialog { Filter = "CSV Files (*.csv)|*.csv", FileName = "AgriConnect_Orders_Report.csv" };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName, _adminCtrl.ExportReportsCsv());
                    MessageBox.Show($"Report exported successfully to {sfd.FileName}", "Export Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }, true));

            pnlExport.Controls.Add(MakeBtn("🚫 Suspend User", () =>
            {
                if (dgvAdminUsers.SelectedRows.Count == 0) return;
                int uId = Convert.ToInt32(dgvAdminUsers.SelectedRows[0].Cells["UserId"].Value);
                _adminCtrl.SetUserStatus(uId, UserStatus.Suspended);
                RefreshAdmin();
            }));

            pnlExport.Controls.Add(MakeBtn("✅ Reactivate User", () =>
            {
                if (dgvAdminUsers.SelectedRows.Count == 0) return;
                int uId = Convert.ToInt32(dgvAdminUsers.SelectedRows[0].Cells["UserId"].Value);
                _adminCtrl.SetUserStatus(uId, UserStatus.Active);
                RefreshAdmin();
            }));

            pnlExport.Controls.Add(MakeBtn("⚖️ Dispute (Refund Buyer)", () =>
            {
                if (dgvAdminDisputes.SelectedRows.Count == 0) return;
                int dId = Convert.ToInt32(dgvAdminDisputes.SelectedRows[0].Cells["DisputeId"].Value);
                _adminCtrl.ResolveDispute(dId, DisputeStatus.ResolvedRefundBuyer, "Admin confirmed crop damage in transit. Escrow refunded to buyer.");
                RefreshAdmin();
            }));

            pnlExport.Controls.Add(MakeBtn("⚖️ Dispute (Release Farmer)", () =>
            {
                if (dgvAdminDisputes.SelectedRows.Count == 0) return;
                int dId = Convert.ToInt32(dgvAdminDisputes.SelectedRows[0].Cells["DisputeId"].Value);
                _adminCtrl.ResolveDispute(dId, DisputeStatus.ResolvedReleaseFarmer, "Admin verified produce met quality standards. Escrow released to farmer.");
                RefreshAdmin();
            }));

            pnl.Controls.Add(pnlExport);

            // User accounts table
            var lblU = new Label { Text = "Registered User Accounts:", Location = new Point(15, 90), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
            pnl.Controls.Add(lblU);

            dgvAdminUsers = new DataGridView
            {
                Location = new Point(15, 115),
                Size = new Size(1005, 150),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvAdminUsers);
            pnl.Controls.Add(dgvAdminUsers);

            // Disputes Table
            var lblD = new Label { Text = "Escrow Disputes & Complaints Room (FR-10.3 / FR-10.4):", Location = new Point(15, 275), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
            pnl.Controls.Add(lblD);

            dgvAdminDisputes = new DataGridView
            {
                Location = new Point(15, 300),
                Size = new Size(1005, 140),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvAdminDisputes);
            pnl.Controls.Add(dgvAdminDisputes);

            // System Audit Logs
            var lblL = new Label { Text = "Administrative Audit Logs (FR-12.6):", Location = new Point(15, 450), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
            pnl.Controls.Add(lblL);

            dgvAdminLogs = new DataGridView
            {
                Location = new Point(15, 475),
                Size = new Size(1005, 160),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvAdminLogs);
            pnl.Controls.Add(dgvAdminLogs);

            return pnl;
        }

        private void RefreshAdmin()
        {
            dgvAdminUsers.DataSource = _adminCtrl.GetAllUsers().Select(u => new
            {
                u.UserId,
                u.FullName,
                u.Email,
                u.Phone,
                u.Role,
                u.District,
                Wallet = $"RWF {u.WalletBalance:N0}",
                u.Status
            }).ToList();

            dgvAdminDisputes.DataSource = _adminCtrl.GetAllDisputes().Select(d => new
            {
                d.DisputeId,
                d.OrderId,
                RaisedBy = d.RaisedByName,
                d.Reason,
                d.Evidence,
                d.Status,
                d.AdminResolutionNotes,
                Date = d.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            }).ToList();

            dgvAdminLogs.DataSource = _adminCtrl.GetAuditLogs().Take(50).Select(l => new
            {
                l.LogId,
                Timestamp = l.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                l.UserName,
                l.Action,
                l.Details
            }).ToList();
        }

        // ==========================================
        // 9. DATABASE GUI INSPECTOR PANEL
        // ==========================================
        private ComboBox cmbDbTables = null!;
        private DataGridView dgvDbViewer = null!;
        private Label lblDbFilePath = null!;
        private Label lblDbStats = null!;
        private TextBox txtDbSearch = null!;

        private Panel CreateDatabasePanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.OffWhite };

            var lblTitle = new Label
            {
                Text = "🗄️ AgriConnect Database Management & GUI Inspector",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(15, 10),
                AutoSize = true
            };
            pnl.Controls.Add(lblTitle);

            lblDbFilePath = new Label
            {
                Text = $"Database File Path: {_db.DatabasePath}",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(15, 36),
                Size = new Size(1000, 20)
            };
            pnl.Controls.Add(lblDbFilePath);

            // Table selector and management toolbar
            var pnlToolbar = new Panel
            {
                Location = new Point(15, 60),
                Size = new Size(1005, 45),
                BackColor = AppTheme.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblT = new Label { Text = "Select Table:", Location = new Point(10, 12), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
            cmbDbTables = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(100, 9), Size = new Size(160, 24), BackColor = AppTheme.OffWhite, ForeColor = AppTheme.TextPrimary };
            cmbDbTables.Items.AddRange(new object[] { "Users", "Listings", "Orders", "Payments", "MarketPrices", "AdvisoryArticles", "FarmerQuestions", "Disputes", "AuditLogs" });
            cmbDbTables.SelectedIndex = 0;
            cmbDbTables.SelectedIndexChanged += (s, e) => LoadSelectedDbTable();

            var lblS = new Label { Text = "Filter Records:", Location = new Point(275, 12), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
            txtDbSearch = new TextBox { Location = new Point(370, 9), Size = new Size(160, 24), BackColor = AppTheme.OffWhite, ForeColor = AppTheme.TextPrimary };
            txtDbSearch.TextChanged += (s, e) => LoadSelectedDbTable();

            Button MakeDbBtn(string text, Action act, int x, int width = 110, bool primary = false)
            {
                var b = new Button
                {
                    Text = text,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Location = new Point(x, 7),
                    Size = new Size(width, 28)
                };
                if (primary) AppTheme.StylePrimaryButton(b);
                else AppTheme.StyleSecondaryButton(b);
                b.Click += (s, e) => act();
                return b;
            }

            var btnRefresh = MakeDbBtn("🔄 Refresh", () => LoadSelectedDbTable(), 550, 80, true);
            var btnBackup = MakeDbBtn("💾 Backup DB", () =>
            {
                using var sfd = new SaveFileDialog { Filter = "JSON Database (*.json)|*.json", FileName = "agriconnect_backup.json" };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    _db.BackupTo(sfd.FileName);
                    MessageBox.Show($"Database successfully backed up to {sfd.FileName}", "Backup Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }, 640);

            var btnRestore = MakeDbBtn("📂 Restore DB", () =>
            {
                using var ofd = new OpenFileDialog { Filter = "JSON Database (*.json)|*.json" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _db.RestoreFrom(ofd.FileName);
                    MessageBox.Show("Database restored successfully!", "Restore Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshCurrentView();
                }
            }, 760);

            var btnReset = MakeDbBtn("⚠️ Reset Defaults", () =>
            {
                if (MessageBox.Show("Reset database back to initial seed data?", "Confirm Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _db.ResetDatabase();
                    MessageBox.Show("Database reset to factory defaults.", "Database Reset", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshCurrentView();
                }
            }, 880, 115);

            // MySQL Students button — opens Form1 (MySQL XAMPP demo)
            var btnMySql = new Button
            {
                Text      = "🐬 MySQL Students",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Location  = new Point(880 + 115 + 10, 7),
                Size      = new Size(140, 28),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
            };
            btnMySql.FlatAppearance.BorderSize = 0;
            btnMySql.Click += (s, e) =>
            {
                var f1 = new AgriConnect.Views.Form1();
                f1.Show();
            };

            pnlToolbar.Size = new Size(1050, 45);
            pnlToolbar.Controls.AddRange(new Control[] { lblT, cmbDbTables, lblS, txtDbSearch, btnRefresh, btnBackup, btnRestore, btnReset, btnMySql });
            pnl.Controls.Add(pnlToolbar);

            lblDbStats = new Label
            {
                Text = "Table Stats: Loading...",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(15, 110),
                Size = new Size(1005, 20)
            };
            pnl.Controls.Add(lblDbStats);

            dgvDbViewer = new DataGridView
            {
                Location = new Point(15, 135),
                Size = new Size(1005, 500),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AppTheme.StyleDataGridView(dgvDbViewer);
            pnl.Controls.Add(dgvDbViewer);

            return pnl;
        }

        private void LoadSelectedDbTable()
        {
            string tableName = cmbDbTables.SelectedItem?.ToString() ?? "Users";
            string search = txtDbSearch.Text.Trim().ToLowerInvariant();

            object? data = null;
            int total = 0;

            switch (tableName)
            {
                case "Users":
                    var uList = _db.Data.Users.Where(u => string.IsNullOrEmpty(search) || u.FullName.ToLower().Contains(search) || u.Email.ToLower().Contains(search) || u.Phone.Contains(search)).ToList();
                    total = _db.Data.Users.Count;
                    data = uList;
                    break;
                case "Listings":
                    var lList = _db.Data.Listings.Where(l => string.IsNullOrEmpty(search) || l.ProduceName.ToLower().Contains(search) || l.District.ToLower().Contains(search)).ToList();
                    total = _db.Data.Listings.Count;
                    data = lList;
                    break;
                case "Orders":
                    var oList = _db.Data.Orders.Where(o => string.IsNullOrEmpty(search) || o.BuyerName.ToLower().Contains(search) || o.FarmerName.ToLower().Contains(search)).ToList();
                    total = _db.Data.Orders.Count;
                    data = oList;
                    break;
                case "Payments":
                    var pList = _db.Data.Payments.Where(p => string.IsNullOrEmpty(search) || p.TransactionReference.ToLower().Contains(search)).ToList();
                    total = _db.Data.Payments.Count;
                    data = pList;
                    break;
                case "MarketPrices":
                    var mpList = _db.Data.MarketPrices.Where(m => string.IsNullOrEmpty(search) || m.CropName.ToLower().Contains(search) || m.MarketName.ToLower().Contains(search)).ToList();
                    total = _db.Data.MarketPrices.Count;
                    data = mpList;
                    break;
                case "AdvisoryArticles":
                    var aList = _db.Data.AdvisoryArticles.Where(a => string.IsNullOrEmpty(search) || a.Title.ToLower().Contains(search)).ToList();
                    total = _db.Data.AdvisoryArticles.Count;
                    data = aList;
                    break;
                case "FarmerQuestions":
                    var qList = _db.Data.FarmerQuestions.Where(q => string.IsNullOrEmpty(search) || q.Subject.ToLower().Contains(search)).ToList();
                    total = _db.Data.FarmerQuestions.Count;
                    data = qList;
                    break;
                case "Disputes":
                    var dList = _db.Data.Disputes.Where(d => string.IsNullOrEmpty(search) || d.Reason.ToLower().Contains(search)).ToList();
                    total = _db.Data.Disputes.Count;
                    data = dList;
                    break;
                case "AuditLogs":
                    var alList = _db.Data.AuditLogs.Where(l => string.IsNullOrEmpty(search) || l.Action.ToLower().Contains(search) || l.Details.ToLower().Contains(search)).ToList();
                    total = _db.Data.AuditLogs.Count;
                    data = alList;
                    break;
            }

            dgvDbViewer.DataSource = null;
            dgvDbViewer.DataSource = data;
            lblDbStats.Text = $"Viewing table: [{tableName}] | Total Records in Database: {total} | Displayed: {dgvDbViewer.Rows.Count}";
        }

        private void RefreshDatabase()
        {
            lblDbFilePath.Text = $"Database File Path: {_db.DatabasePath} (Saved to disk: {File.Exists(_db.DatabasePath)})";
            LoadSelectedDbTable();
        }
    }
}
