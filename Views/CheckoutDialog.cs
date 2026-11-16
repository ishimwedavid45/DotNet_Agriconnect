using AgriConnect.Controllers;
using AgriConnect.Helpers;
using AgriConnect.Models;

namespace AgriConnect.Views
{
    public class CheckoutDialog : Form
    {
        private readonly BuyerController _controller = new();
        public Order? CreatedOrder { get; private set; }

        private RadioButton rbDelivery = null!;
        private RadioButton rbPickup = null!;
        private TextBox txtAddress = null!;
        private ComboBox cmbPaymentMethod = null!;
        private Label lblProduceTotal = null!;
        private Label lblDeliveryFee = null!;
        private Label lblPlatformFee = null!;
        private Label lblGrandTotal = null!;

        public CheckoutDialog()
        {
            InitializeComponent();
            UpdateCalculations();
        }

        private void InitializeComponent()
        {
            Text = "AgriConnect Secure Escrow Checkout";
            Size = new Size(580, 680);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = AppTheme.White;
            Font = new Font("Segoe UI", 9.5f);

            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = AppTheme.Charcoal
            };
            var lblTitle = new Label
            {
                Text = "🛒 Order Checkout & Escrow Payment",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = AppTheme.White,
                Location = new Point(20, 12),
                AutoSize = true
            };
            var lblSub = new Label
            {
                Text = "100% Protected: Farmer receives payment only after you confirm delivery.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = AppTheme.SlateBorder,
                Location = new Point(22, 40),
                AutoSize = true
            };
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSub });

            // Cart Items list box
            var lblCartTitle = new Label
            {
                Text = "Order Items:",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(25, 80),
                AutoSize = true
            };

            var lstItems = new ListBox
            {
                Location = new Point(25, 105),
                Size = new Size(515, 110),
                Font = new Font("Consolas", 9f),
                IntegralHeight = false,
                BackColor = AppTheme.OffWhite,
                ForeColor = AppTheme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            foreach (var item in AppSession.Cart)
            {
                lstItems.Items.Add($"{item.ProduceName,-28} {item.Quantity,6} {item.Unit,-4} @ RWF {item.UnitPrice,6:N0} = RWF {item.Subtotal,9:N0}");
            }

            // Delivery method
            var grpDelivery = new GroupBox
            {
                Text = "Delivery Method",
                Location = new Point(25, 225),
                Size = new Size(515, 130),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                BackColor = AppTheme.White
            };

            rbDelivery = new RadioButton
            {
                Text = "Transporter Delivery (+ RWF 5,000 flat courier fee)",
                Checked = true,
                Location = new Point(15, 25),
                Size = new Size(450, 24),
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = AppTheme.TextPrimary
            };
            rbDelivery.CheckedChanged += (s, e) => UpdateCalculations();

            rbPickup = new RadioButton
            {
                Text = "Self-Pickup at Farmer Collection Station (Free)",
                Location = new Point(15, 50),
                Size = new Size(450, 24),
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = AppTheme.TextPrimary
            };
            rbPickup.CheckedChanged += (s, e) => UpdateCalculations();

            var lblAddr = new Label
            {
                Text = "Destination Address:",
                Location = new Point(15, 85),
                Size = new Size(130, 20),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = AppTheme.TextSecondary
            };
            txtAddress = new TextBox
            {
                Text = AppSession.CurrentUser?.DeliveryAddress ?? "KG 11 Ave, Kimironko Plaza, Kigali",
                Location = new Point(150, 82),
                Size = new Size(345, 24),
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                BackColor = AppTheme.OffWhite,
                ForeColor = AppTheme.TextPrimary
            };
            grpDelivery.Controls.AddRange(new Control[] { rbDelivery, rbPickup, lblAddr, txtAddress });

            // Payment method
            var grpPayment = new GroupBox
            {
                Text = "Payment Gateway (Held in Escrow)",
                Location = new Point(25, 365),
                Size = new Size(515, 75),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                BackColor = AppTheme.White
            };
            var lblPayMethod = new Label
            {
                Text = "Select Gateway:",
                Location = new Point(15, 30),
                Size = new Size(120, 24),
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = AppTheme.TextSecondary
            };
            cmbPaymentMethod = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(150, 28),
                Size = new Size(345, 26),
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                BackColor = AppTheme.OffWhite,
                ForeColor = AppTheme.TextPrimary
            };
            cmbPaymentMethod.Items.AddRange(new object[] { "MTN MoMo (*182#)", "Airtel Money (*500#)", "Bank Card / Visa / MasterCard" });
            cmbPaymentMethod.SelectedIndex = 0;
            grpPayment.Controls.AddRange(new Control[] { lblPayMethod, cmbPaymentMethod });

            // Financial Summary
            var pnlSummary = new Panel
            {
                Location = new Point(25, 450),
                Size = new Size(515, 120),
                BackColor = AppTheme.LightGrey,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblProduceTotal = new Label { Location = new Point(15, 12), Size = new Size(480, 20), Text = "Produce Cost: RWF 0", ForeColor = AppTheme.TextPrimary };
            lblDeliveryFee = new Label { Location = new Point(15, 34), Size = new Size(480, 20), Text = "Delivery Fee: RWF 0", ForeColor = AppTheme.TextPrimary };
            lblPlatformFee = new Label { Location = new Point(15, 56), Size = new Size(480, 20), Text = "Escrow & Platform Fee (3%): RWF 0", ForeColor = AppTheme.TextPrimary };
            lblGrandTotal = new Label
            {
                Location = new Point(15, 82),
                Size = new Size(480, 26),
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = AppTheme.Charcoal,
                Text = "TOTAL AMOUNT: RWF 0"
            };
            pnlSummary.Controls.AddRange(new Control[] { lblProduceTotal, lblDeliveryFee, lblPlatformFee, lblGrandTotal });

            // Buttons
            var btnConfirm = new Button
            {
                Text = "🔒 Confirm & Pay via Escrow",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Location = new Point(180, 585),
                Size = new Size(220, 40)
            };
            AppTheme.StylePrimaryButton(btnConfirm);
            btnConfirm.Click += BtnConfirm_Click;

            var btnCancel = new Button
            {
                Text = "Cancel",
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(410, 585),
                Size = new Size(130, 40)
            };
            AppTheme.StyleSecondaryButton(btnCancel);
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] { pnlHeader, lblCartTitle, lstItems, grpDelivery, grpPayment, pnlSummary, btnConfirm, btnCancel });
        }

        private void UpdateCalculations()
        {
            var delType = rbDelivery.Checked ? DeliveryType.TransporterDelivery : DeliveryType.SelfPickup;
            var (produce, delivery, platform, total) = _controller.CalculateTotals(delType);

            lblProduceTotal.Text = $"Produce Subtotal: RWF {produce:N0}";
            lblDeliveryFee.Text = $"Logistics / Delivery Fee: RWF {delivery:N0}";
            lblPlatformFee.Text = $"Platform Escrow Assurance (3%): RWF {platform:N0}";
            lblGrandTotal.Text = $"TOTAL PAYABLE: RWF {total:N0}";
        }

        private void BtnConfirm_Click(object? sender, EventArgs e)
        {
            var buyer = AppSession.CurrentUser;
            if (buyer == null)
            {
                MessageBox.Show("Please log in as a buyer to checkout.", "Authentication", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var delType = rbDelivery.Checked ? DeliveryType.TransporterDelivery : DeliveryType.SelfPickup;
            var method = cmbPaymentMethod.SelectedIndex switch
            {
                1 => PaymentMethod.AirtelMoney,
                2 => PaymentMethod.BankCard,
                _ => PaymentMethod.MTN_MoMo
            };

            var (success, msg, order) = _controller.Checkout(buyer, delType, txtAddress.Text.Trim(), method);
            if (success && order != null)
            {
                CreatedOrder = order;
                MessageBox.Show(
                    $"{msg}\n\nOrder Code: #{order.OrderId}\nSecurity Delivery Code: {order.DeliveryCode}\n" +
                    "Keep your Delivery Code safe! Provide it to the transporter only when you inspect and receive your produce.",
                    "Escrow Payment Successful",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(msg, "Checkout Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
