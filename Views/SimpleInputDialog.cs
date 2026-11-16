using AgriConnect.Helpers;

namespace AgriConnect.Views
{
    public class SimpleInputDialog : Form
    {
        private TextBox txtInput = null!;
        public string InputText => txtInput.Text;

        public SimpleInputDialog(string title, string prompt, string defaultValue = "")
        {
            Text = title;
            Size = new Size(460, 220);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = AppTheme.White;
            Font = new Font("Segoe UI", 9.5f);

            var lblPrompt = new Label
            {
                Text = prompt,
                Location = new Point(20, 20),
                Size = new Size(405, 45),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary
            };

            txtInput = new TextBox
            {
                Text = defaultValue,
                Location = new Point(20, 75),
                Size = new Size(405, 30),
                Font = new Font("Segoe UI", 10.5f),
                BackColor = AppTheme.OffWhite,
                ForeColor = AppTheme.TextPrimary
            };

            var btnOk = new Button
            {
                Text = "Confirm",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(215, 125),
                Size = new Size(100, 34),
                DialogResult = DialogResult.OK
            };
            AppTheme.StylePrimaryButton(btnOk);

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(325, 125),
                Size = new Size(100, 34),
                DialogResult = DialogResult.Cancel
            };
            AppTheme.StyleSecondaryButton(btnCancel);

            Controls.AddRange(new Control[] { lblPrompt, txtInput, btnOk, btnCancel });
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        public static string Prompt(string title, string prompt, string defaultValue = "")
        {
            using var dlg = new SimpleInputDialog(title, prompt, defaultValue);
            return dlg.ShowDialog() == DialogResult.OK ? dlg.InputText : string.Empty;
        }
    }
}
