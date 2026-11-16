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
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9.5f);

            var lblPrompt = new Label
            {
                Text = prompt,
                Location = new Point(20, 20),
                Size = new Size(405, 45),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            txtInput = new TextBox
            {
                Text = defaultValue,
                Location = new Point(20, 75),
                Size = new Size(405, 30),
                Font = new Font("Segoe UI", 10.5f)
            };

            var btnOk = new Button
            {
                Text = "Confirm",
                BackColor = Color.FromArgb(46, 125, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(215, 125),
                Size = new Size(100, 34),
                DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Text = "Cancel",
                BackColor = Color.FromArgb(235, 235, 235),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(325, 125),
                Size = new Size(100, 34),
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

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
