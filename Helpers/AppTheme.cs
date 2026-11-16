namespace AgriConnect.Helpers
{
    public static class AppTheme
    {
        // Core Professional Grey & White Palette
        public static readonly Color White = Color.FromArgb(255, 255, 255);
        public static readonly Color OffWhite = Color.FromArgb(248, 249, 250);      // #F8F9FA
        public static readonly Color LightGrey = Color.FromArgb(241, 243, 245);     // #F1F3F5
        public static readonly Color BorderGrey = Color.FromArgb(222, 226, 230);    // #DEE2E6
        public static readonly Color SlateBorder = Color.FromArgb(206, 212, 218);   // #CED4DA
        public static readonly Color MutedGrey = Color.FromArgb(108, 117, 125);     // #6C757D
        public static readonly Color MediumGrey = Color.FromArgb(73, 80, 87);       // #495057
        public static readonly Color DarkGrey = Color.FromArgb(52, 58, 64);         // #343A40
        public static readonly Color Charcoal = Color.FromArgb(33, 37, 41);         // #212529
        public static readonly Color DeepCharcoal = Color.FromArgb(24, 28, 33);     // #181C21

        // Text Colors
        public static readonly Color TextPrimary = Color.FromArgb(33, 37, 41);
        public static readonly Color TextSecondary = Color.FromArgb(108, 117, 125);
        public static readonly Color TextLight = Color.FromArgb(248, 249, 250);

        // Helper button stylers
        public static void StylePrimaryButton(Button btn, string text = "")
        {
            if (!string.IsNullOrEmpty(text)) btn.Text = text;
            btn.BackColor = DarkGrey;
            btn.ForeColor = White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Charcoal;
            btn.Cursor = Cursors.Hand;
        }

        public static void StyleSecondaryButton(Button btn, string text = "")
        {
            if (!string.IsNullOrEmpty(text)) btn.Text = text;
            btn.BackColor = LightGrey;
            btn.ForeColor = TextPrimary;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = SlateBorder;
            btn.FlatAppearance.MouseOverBackColor = BorderGrey;
            btn.Cursor = Cursors.Hand;
        }

        public static void StyleMutedButton(Button btn, string text = "")
        {
            if (!string.IsNullOrEmpty(text)) btn.Text = text;
            btn.BackColor = MediumGrey;
            btn.ForeColor = White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = DarkGrey;
            btn.Cursor = Cursors.Hand;
        }

        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = BorderGrey;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = DarkGrey;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 34;
            dgv.RowHeadersVisible = false;
            dgv.DefaultCellStyle.BackColor = White;
            dgv.DefaultCellStyle.ForeColor = TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = LightGrey;
            dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = OffWhite;
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = TextPrimary;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = LightGrey;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = TextPrimary;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
        }
    }
}
