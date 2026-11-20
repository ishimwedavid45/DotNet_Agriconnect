using AgriConnect.Controllers;
using AgriConnect.Helpers;
using AgriConnect.Models;

namespace AgriConnect.Views
{
    /// <summary>
    /// Form1 — MySQL Students Demo (MVC View).
    ///
    /// Layout (all built in code — no .Designer.cs needed):
    ///   ┌─ Header panel ───────────────────────────────────────────────────┐
    ///   │  AgriConnect · Students (MySQL)                                  │
    ///   ├─ Input panel ────────────────────────────────────────────────────┤
    ///   │  Student Name: [___________]  Course: [___________]              │
    ///   │  Email:        [___________]  [  Save Student  ]                 │
    ///   ├─ Status bar ─────────────────────────────────────────────────────┤
    ///   │  lblStatus                                                       │
    ///   ├─ Grid ───────────────────────────────────────────────────────────┤
    ///   │  DataGridView (all rows from MySQL students table)               │
    ///   └──────────────────────────────────────────────────────────────────┘
    /// </summary>
    public class Form1 : Form
    {
        // ── MVC: controller ───────────────────────────────────────────────
        private readonly StudentController _controller = new();

        // ── Controls ─────────────────────────────────────────────────────
        private TextBox    txtStudentName = null!;
        private TextBox    txtCourse      = null!;
        private TextBox    txtEmail       = null!;
        private Button     btnSave        = null!;
        private Label      lblStatus      = null!;
        private DataGridView dgvStudents  = null!;

        // ─────────────────────────────────────────────────────────────────
        public Form1()
        {
            InitializeComponent();
            ApplyTheme();
            LoadStudents();          // show existing rows immediately
        }

        // ── Build UI entirely in code (no Designer.cs required) ──────────
        private void InitializeComponent()
        {
            Text            = "AgriConnect – Students (MySQL XAMPP Demo)";
            Size            = new Size(900, 600);
            MinimumSize     = new Size(760, 500);
            StartPosition   = FormStartPosition.CenterScreen;
            Font            = new Font("Segoe UI", 10f);

            // ── Header ──────────────────────────────────────────────────
            _pnlHeader = new Panel
            {
                Dock   = DockStyle.Top,
                Height = 56,
            };

            var lblTitle = new Label
            {
                Text      = "🌾  AgriConnect  –  Students Database",
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font      = new Font("Segoe UI", 15f, FontStyle.Bold),
                Padding   = new Padding(16, 0, 0, 0),
            };
            _pnlHeader.Controls.Add(lblTitle);

            // ── Input panel ─────────────────────────────────────────────
            _pnlInput = new Panel
            {
                Dock    = DockStyle.Top,
                Height  = 110,
                Padding = new Padding(16, 10, 16, 10),
            };

            // Row 1: Name + Course
            var lblName = MakeLabel("Student Name:", 0, 14);
            txtStudentName = MakeTextBox(110, 14, 240);

            var lblCourse = MakeLabel("Course:", 370, 14);
            txtCourse     = MakeTextBox(430, 14, 240);

            // Row 2: Email + Save button
            var lblEmail = MakeLabel("Email:", 0, 56);
            txtEmail     = MakeTextBox(110, 56, 240);

            btnSave = new Button
            {
                Text     = "💾  Save Student",
                Location = new Point(370, 52),
                Size     = new Size(150, 36),
                Cursor   = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
            };
            btnSave.Click += BtnSave_Click;

            _pnlInput.Controls.AddRange(new Control[]
            {
                lblName, txtStudentName,
                lblCourse, txtCourse,
                lblEmail, txtEmail,
                btnSave,
            });

            // ── Status bar ──────────────────────────────────────────────
            lblStatus = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 28,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(16, 0, 0, 0),
                Text      = "Ready — connected to agriconnect_db on localhost:3306",
            };

            // ── DataGridView ─────────────────────────────────────────────
            dgvStudents = new DataGridView
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

            // ── Compose form ─────────────────────────────────────────────
            // Note: add in reverse Dock order (Fill goes last / Bottom → Top order matters)
            Controls.Add(dgvStudents);    // Fill  (added first so Fill expands)
            Controls.Add(lblStatus);      // Top   (added after Fill)
            Controls.Add(_pnlInput);      // Top
            Controls.Add(_pnlHeader);     // Top
        }

        // ── Helper factories ──────────────────────────────────────────────
        private static Label MakeLabel(string text, int x, int y) => new Label
        {
            Text     = text,
            Location = new Point(x, y + 4),
            AutoSize = true,
        };

        private static TextBox MakeTextBox(int x, int y, int width) => new TextBox
        {
            Location  = new Point(x, y),
            Size      = new Size(width, 28),
            MaxLength = 150,
        };

        // ── Theme ─────────────────────────────────────────────────────────
        private Panel  _pnlHeader = null!;
        private Panel  _pnlInput  = null!;

        private void ApplyTheme()
        {
            BackColor = AppTheme.OffWhite;
            ForeColor = AppTheme.Charcoal;

            // Header
            _pnlHeader.BackColor = AppTheme.DarkGrey;
            _pnlHeader.ForeColor = Color.White;
            foreach (Control c in _pnlHeader.Controls)
                c.ForeColor = Color.White;

            // Input panel
            _pnlInput.BackColor = AppTheme.LightGrey;

            // Status bar
            lblStatus.BackColor = AppTheme.MediumGrey;
            lblStatus.ForeColor = AppTheme.OffWhite;

            // Save button (use a teal/green tone inline — AppTheme is grey/white focused)
            btnSave.BackColor = Color.FromArgb(40, 167, 69);   // Bootstrap success green
            btnSave.ForeColor = Color.White;
            btnSave.FlatAppearance.BorderSize = 0;

            // DataGridView
            dgvStudents.BackgroundColor      = AppTheme.OffWhite;
            dgvStudents.GridColor            = AppTheme.LightGrey;
            dgvStudents.DefaultCellStyle.BackColor           = AppTheme.White;
            dgvStudents.DefaultCellStyle.ForeColor           = AppTheme.Charcoal;
            dgvStudents.AlternatingRowsDefaultCellStyle.BackColor = AppTheme.LightGrey;
            dgvStudents.ColumnHeadersDefaultCellStyle.BackColor   = AppTheme.DarkGrey;
            dgvStudents.ColumnHeadersDefaultCellStyle.ForeColor   = Color.White;
            dgvStudents.ColumnHeadersDefaultCellStyle.Font        = new Font("Segoe UI", 10f, FontStyle.Bold);
            dgvStudents.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvStudents.EnableHeadersVisualStyles = false;
        }

        // ── Event handlers ────────────────────────────────────────────────

        /// <summary>Save button: validate → parameterized INSERT → reload grid.</summary>
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            lblStatus.ForeColor = AppTheme.OffWhite;

            try
            {
                var students = _controller.SaveStudent(
                    txtStudentName.Text,
                    txtCourse.Text,
                    txtEmail.Text);

                BindGrid(students);

                // Clear inputs for the next entry
                txtStudentName.Clear();
                txtCourse.Clear();
                txtEmail.Clear();
                txtStudentName.Focus();

                lblStatus.Text = $"✅  Student saved successfully. Total rows: {students.Count}";
            }
            catch (ArgumentException ex)
            {
                // Validation error — show inline, not a dialog
                lblStatus.ForeColor = Color.OrangeRed;
                lblStatus.Text = $"⚠  {ex.Message}";
            }
            catch (MySqlConnector.MySqlException ex)
            {
                lblStatus.ForeColor = Color.OrangeRed;
                lblStatus.Text = $"❌  MySQL error: {ex.Message}";
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.OrangeRed;
                lblStatus.Text = $"❌  Unexpected error: {ex.Message}";
            }
        }

        // ── Data helpers ──────────────────────────────────────────────────

        /// <summary>Loads all students from MySQL and binds them to the DataGridView.</summary>
        private void LoadStudents()
        {
            try
            {
                BindGrid(_controller.GetAllStudents());
                lblStatus.Text = "Ready — connected to agriconnect_db on localhost:3306";
            }
            catch (MySqlConnector.MySqlException ex)
            {
                lblStatus.ForeColor = Color.OrangeRed;
                lblStatus.Text = $"❌  Cannot connect to MySQL: {ex.Message}  " +
                                 "— Make sure XAMPP MySQL is running.";
            }
        }

        /// <summary>Binds a list of Student objects to the DataGridView.</summary>
        private void BindGrid(List<Student> students)
        {
            // Use BindingList so the grid reflects the list directly
            dgvStudents.DataSource = null;
            dgvStudents.DataSource = students;

            // Rename columns for display
            if (dgvStudents.Columns.Count > 0)
            {
                dgvStudents.Columns["Id"]!.HeaderText           = "ID";
                dgvStudents.Columns["StudentName"]!.HeaderText  = "Student Name";
                dgvStudents.Columns["Course"]!.HeaderText       = "Course";
                dgvStudents.Columns["Email"]!.HeaderText        = "Email";
                dgvStudents.Columns["EnrolledDate"]!.HeaderText = "Enrolled Date";

                // Fix column widths
                dgvStudents.Columns["Id"]!.FillWeight           = 8;
                dgvStudents.Columns["StudentName"]!.FillWeight  = 22;
                dgvStudents.Columns["Course"]!.FillWeight       = 22;
                dgvStudents.Columns["Email"]!.FillWeight        = 28;
                dgvStudents.Columns["EnrolledDate"]!.FillWeight = 20;
            }
        }
    }
}
