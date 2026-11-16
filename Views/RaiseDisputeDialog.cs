using AgriConnect.Helpers;
using AgriConnect.Models;
using AgriConnect.Repositories;

namespace AgriConnect.Views
{
    public class RaiseDisputeDialog : Form
    {
        private readonly IDisputeRepository _repo = new DisputeRepository();
        public Dispute? CreatedDispute { get; private set; }

        private readonly int _orderId;
        private TextBox txtReason = null!;
        private TextBox txtEvidence = null!;

        public RaiseDisputeDialog(int orderId)
        {
            _orderId = orderId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = $"Open Dispute for Order #{_orderId}";
            Size = new Size(500, 420);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = AppTheme.White;
            Font = new Font("Segoe UI", 9.5f);

            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = AppTheme.Charcoal
            };
            var lblTitle = new Label
            {
                Text = $"⚠️ Open Formal Dispute (Order #{_orderId})",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = AppTheme.White,
                Location = new Point(15, 12),
                AutoSize = true
            };
            var lblSub = new Label
            {
                Text = "AgriConnect Admin will arbitrate and freeze escrow release until resolved.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = AppTheme.SlateBorder,
                Location = new Point(18, 36),
                AutoSize = true
            };
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSub });

            var lblReason = new Label
            {
                Text = "Reason for Dispute / Issue Description *:",
                Location = new Point(20, 75),
                Size = new Size(400, 20),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary
            };
            txtReason = new TextBox
            {
                Location = new Point(20, 100),
                Size = new Size(440, 60),
                Multiline = true,
                BackColor = AppTheme.OffWhite,
                ForeColor = AppTheme.TextPrimary
            };

            var lblEv = new Label
            {
                Text = "Evidence / Details (damaged produce, wrong weight, etc.):",
                Location = new Point(20, 175),
                Size = new Size(440, 20),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary
            };
            txtEvidence = new TextBox
            {
                Location = new Point(20, 200),
                Size = new Size(440, 100),
                Multiline = true,
                BackColor = AppTheme.OffWhite,
                ForeColor = AppTheme.TextPrimary
            };

            var btnSubmit = new Button
            {
                Text = "Submit Dispute",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Location = new Point(200, 320),
                Size = new Size(150, 36)
            };
            AppTheme.StylePrimaryButton(btnSubmit);
            btnSubmit.Click += BtnSubmit_Click;

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(360, 320),
                Size = new Size(100, 36)
            };
            AppTheme.StyleSecondaryButton(btnCancel);
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] { pnlHeader, lblReason, txtReason, lblEv, txtEvidence, btnSubmit, btnCancel });
        }

        private void BtnSubmit_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                MessageBox.Show("Please enter the reason for the dispute.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dispute = new Dispute
            {
                OrderId = _orderId,
                RaisedByUserId = Controllers.AppSession.CurrentUser?.UserId ?? 0,
                RaisedByName = Controllers.AppSession.CurrentUser?.FullName ?? "User",
                Reason = txtReason.Text.Trim(),
                Evidence = txtEvidence.Text.Trim(),
                Status = DisputeStatus.Open
            };

            CreatedDispute = _repo.Create(dispute);
            MessageBox.Show($"Dispute #{CreatedDispute.DisputeId} filed. Escrow funds are frozen pending administrative review.", "Dispute Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
