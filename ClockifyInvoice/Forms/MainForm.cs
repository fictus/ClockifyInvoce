using ClockifyInvoice.Models;
using ClockifyInvoice.Services;

namespace ClockifyInvoice.Forms
{
    public class MainForm : Form
    {
        private readonly ProfileService _profileService;
        private readonly ClockifyCsvParser _csvParser;
        private readonly PdfInvoiceGenerator _pdfGenerator;

        private InvoiceProfile _profile = null!;
        private List<ClockifyRecord> _records = new();
        private DataGridView dgvRecords = null!;
        private DataGridView dgvLineItems = null!;

        // Top controls
        private Label lblCsvFile = null!, lblStatus = null!;
        private Button btnLoadCsv = null!, btnEditProfile = null!, btnGenerateInvoice = null!;
        private Label lblInvoiceNum = null!, lblTotal = null!;
        private DateTimePicker dtpInvoiceDate = null!;
        private Panel pnlTop = null!, pnlBottom = null!, pnlSplit = null!;

        public MainForm()
        {
            _profileService = new ProfileService();
            _csvParser = new ClockifyCsvParser();
            _pdfGenerator = new PdfInvoiceGenerator();
            _profile = _profileService.Load();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Clockify Invoice Generator";
            Size = new Size(1060, 720);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9f);

            // ── TOP PANEL ──────────────────────────────────────────────────────
            pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(30, 35, 50),
                Padding = new Padding(16, 0, 16, 0)
            };

            var lblTitle = new Label
            {
                Text = "⚡ Clockify Invoice Generator",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 20)
            };
            pnlTop.Controls.Add(lblTitle);

            btnEditProfile = new Button
            {
                Text = "⚙  Edit Profile",
                Location = new Point(700, 22),
                Size = new Size(130, 36),
                BackColor = Color.FromArgb(60, 70, 95),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Cursor = Cursors.Hand
            };
            btnEditProfile.FlatAppearance.BorderSize = 0;
            btnEditProfile.Click += BtnEditProfile_Click;
            pnlTop.Controls.Add(btnEditProfile);

            btnGenerateInvoice = new Button
            {
                Text = "📄  Generate PDF Invoice",
                Location = new Point(844, 22),
                Size = new Size(190, 36),
                BackColor = Color.FromArgb(44, 95, 138),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnGenerateInvoice.FlatAppearance.BorderSize = 0;
            btnGenerateInvoice.Click += BtnGenerateInvoice_Click;
            pnlTop.Controls.Add(btnGenerateInvoice);

            // ── TOOLBAR PANEL ──────────────────────────────────────────────────
            var pnlToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(16, 0, 16, 0)
            };
            pnlToolbar.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 225, 235)), 0, pnlToolbar.Height - 1, pnlToolbar.Width, pnlToolbar.Height - 1);
            };

            btnLoadCsv = new Button
            {
                Text = "📂  Load Clockify CSV",
                Location = new Point(16, 14),
                Size = new Size(170, 34),
                BackColor = Color.FromArgb(44, 95, 138),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Cursor = Cursors.Hand
            };
            btnLoadCsv.FlatAppearance.BorderSize = 0;
            btnLoadCsv.Click += BtnLoadCsv_Click;

            lblCsvFile = new Label
            {
                Text = "No file loaded",
                Location = new Point(200, 20),
                Size = new Size(380, 20),
                ForeColor = Color.FromArgb(120, 130, 150),
                Font = new Font("Segoe UI", 9f)
            };

            var lblDateLabel = new Label
            {
                Text = "Invoice Date:",
                Location = new Point(590, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(80, 90, 110)
            };

            dtpInvoiceDate = new DateTimePicker
            {
                Location = new Point(680, 14),
                Size = new Size(160, 34),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today,
                Font = new Font("Segoe UI", 9.5f)
            };

            lblInvoiceNum = new Label
            {
                Text = $"Next: {_profile.FormattedInvoiceNumber}",
                Location = new Point(850, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(44, 95, 138),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            pnlToolbar.Controls.AddRange(new Control[] { btnLoadCsv, lblCsvFile, lblDateLabel, dtpInvoiceDate, lblInvoiceNum });

            // ── STATUS / TOTAL BAR ─────────────────────────────────────────────
            pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                BackColor = Color.FromArgb(30, 35, 50)
            };

            lblStatus = new Label
            {
                Text = "Ready. Load a Clockify CSV export to begin.",
                Location = new Point(16, 10),
                Size = new Size(500, 18),
                ForeColor = Color.FromArgb(160, 175, 200),
                Font = new Font("Segoe UI", 8.5f)
            };

            lblTotal = new Label
            {
                Text = "",
                Location = new Point(700, 10),
                Size = new Size(330, 18),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            pnlBottom.Controls.AddRange(new Control[] { lblStatus, lblTotal });

            // ── SPLIT CONTENT ──────────────────────────────────────────────────
            pnlSplit = new Panel { Dock = DockStyle.Fill };

            // Top half: raw records
            var pnlRaw = new Panel { Dock = DockStyle.Top, Height = 200, Padding = new Padding(16, 12, 16, 0) };
            var lblRaw = new Label
            {
                Text = "RAW RECORDS FROM CSV",
                Dock = DockStyle.Top,
                Height = 22,
                ForeColor = Color.FromArgb(44, 95, 138),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };

            dgvRecords = CreateGrid();
            dgvRecords.Dock = DockStyle.Fill;
            pnlRaw.Controls.Add(dgvRecords);
            pnlRaw.Controls.Add(lblRaw);

            // Splitter
            var splitter = new Splitter { Dock = DockStyle.Top, Height = 6, BackColor = Color.FromArgb(220, 225, 235) };

            // Bottom half: invoice line items
            var pnlInvoice = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 8, 16, 0) };
            var lblInv = new Label
            {
                Text = "INVOICE LINE ITEMS (grouped by project + description)",
                Dock = DockStyle.Top,
                Height = 22,
                ForeColor = Color.FromArgb(44, 95, 138),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };

            dgvLineItems = CreateGrid();
            dgvLineItems.Dock = DockStyle.Fill;
            pnlInvoice.Controls.Add(dgvLineItems);
            pnlInvoice.Controls.Add(lblInv);

            pnlSplit.Controls.Add(pnlInvoice);
            pnlSplit.Controls.Add(splitter);
            pnlSplit.Controls.Add(pnlRaw);

            // ── ASSEMBLY ───────────────────────────────────────────────────────
            Controls.Add(pnlSplit);
            Controls.Add(pnlBottom);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlTop);
        }

        private DataGridView CreateGrid()
        {
            var dgv = new DataGridView
            {
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Font = new Font("Segoe UI", 9f),
                GridColor = Color.FromArgb(230, 233, 240)
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 138);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
            return dgv;
        }

        private void BtnLoadCsv_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select Clockify CSV Export",
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                _records = _csvParser.Parse(dlg.FileName);
                lblCsvFile.Text = Path.GetFileName(dlg.FileName);

                // Populate raw grid
                dgvRecords.DataSource = null;
                var dt = new System.Data.DataTable();
                dt.Columns.AddRange(new[]
                {
                    new System.Data.DataColumn("Date"),
                    new System.Data.DataColumn("Project"),
                    new System.Data.DataColumn("Client"),
                    new System.Data.DataColumn("Description"),
                    new System.Data.DataColumn("Billable"),
                    new System.Data.DataColumn("Duration (h)"),
                    new System.Data.DataColumn("Hours"),
                    new System.Data.DataColumn("Rate"),
                    new System.Data.DataColumn("Amount"),
                });
                foreach (var r in _records)
                {
                    dt.Rows.Add(r.StartDate, r.Project, r.Client, r.Description,
                        r.Billable, r.DurationH, r.DurationDecimal.ToString("F2"),
                        $"{_profile.CurrencySymbol}{r.BillableRate:F2}",
                        $"{_profile.CurrencySymbol}{r.BillableAmount:F2}");
                }
                dgvRecords.DataSource = dt;

                // Build line items
                BuildLineItems();
                btnGenerateInvoice.Enabled = true;
                lblStatus.Text = $"Loaded {_records.Count} records from {Path.GetFileName(dlg.FileName)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading CSV:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BuildLineItems()
        {
            var billableRecords = _records.Where(r => r.Billable?.Equals("Yes", StringComparison.OrdinalIgnoreCase) == true).ToList();

            // Group by project + description, sum hours
            var lineItems = billableRecords
                .GroupBy(r => new { r.Project, r.Description })
                .Select(g =>
                {
                    var rate = g.Max(r => r.BillableRate) > 0 ? g.Max(r => r.BillableRate) : _profile.DefaultHourlyRate;
                    return new InvoiceLineItem
                    {
                        Date = g.Min(r => r.StartDate),
                        Project = g.Key.Project,
                        Description = g.Key.Description,
                        Hours = g.Sum(r => r.DurationDecimal),
                        Rate = rate
                    };
                })
                .OrderBy(i => i.Date)
                .ThenBy(i => i.Project)
                .ToList();

            dgvLineItems.DataSource = null;
            var dt2 = new System.Data.DataTable();
            dt2.Columns.AddRange(new[]
            {
                new System.Data.DataColumn("Date"),
                new System.Data.DataColumn("Project"),
                new System.Data.DataColumn("Description"),
                new System.Data.DataColumn("Hours"),
                new System.Data.DataColumn("Rate"),
                new System.Data.DataColumn("Amount"),
            });
            foreach (var item in lineItems)
            {
                dt2.Rows.Add(item.Date, item.Project, item.Description,
                    item.Hours.ToString("F2"),
                    $"{_profile.CurrencySymbol}{item.Rate:F2}",
                    $"{_profile.CurrencySymbol}{item.Amount:F2}");
            }
            dgvLineItems.DataSource = dt2;

            var subtotal = lineItems.Sum(i => i.Amount);
            var tax = subtotal * (_profile.TaxRate / 100m);
            var total = subtotal + tax;
            lblTotal.Text = $"Subtotal: {_profile.CurrencySymbol}{subtotal:F2}   |   Total: {_profile.CurrencySymbol}{total:F2}";
        }

        private void BtnEditProfile_Click(object? sender, EventArgs e)
        {
            using var form = new ProfileEditorForm(_profileService);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _profile = _profileService.Load();
                lblInvoiceNum.Text = $"Next: {_profile.FormattedInvoiceNumber}";
                if (_records.Count > 0) BuildLineItems();
            }
        }

        private void BtnGenerateInvoice_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_profile.FreelancerName))
            {
                MessageBox.Show("Please set up your profile first (click ⚙ Edit Profile).", "Profile Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Title = "Save Invoice PDF",
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"Invoice_{_profile.FormattedInvoiceNumber}_{DateTime.Today:yyyy-MM-dd}.pdf"
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                var billableRecords = _records.Where(r => r.Billable?.Equals("Yes", StringComparison.OrdinalIgnoreCase) == true).ToList();
                var lineItems = billableRecords
                    .GroupBy(r => new { r.Project, r.Description })
                    .Select(g =>
                    {
                        var rate = g.Max(r => r.BillableRate) > 0 ? g.Max(r => r.BillableRate) : _profile.DefaultHourlyRate;
                        return new InvoiceLineItem
                        {
                            Date = g.Min(r => r.StartDate),
                            Project = g.Key.Project,
                            Description = g.Key.Description,
                            Hours = g.Sum(r => r.DurationDecimal),
                            Rate = rate,
                            PrecomputedAmount = g.Sum(r => r.BillableAmount) // ← use Clockify's amount
                        };
                    })
                    .OrderBy(i => i.Date)
                    .ThenBy(i => i.Project)
                    .ToList();

                var invoiceNumber = _profile.FormattedInvoiceNumber;
                _pdfGenerator.Generate(dlg.FileName, _profile, lineItems, invoiceNumber, dtpInvoiceDate.Value);

                // Increment invoice number
                _profileService.IncrementInvoiceNumber(_profile);
                _profile = _profileService.Load();
                lblInvoiceNum.Text = $"Next: {_profile.FormattedInvoiceNumber}";

                lblStatus.Text = $"✅ Invoice {invoiceNumber} saved to {Path.GetFileName(dlg.FileName)}";

                var result = MessageBox.Show(
                    $"Invoice {invoiceNumber} generated successfully!\n\nOpen the PDF now?",
                    "Success",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
