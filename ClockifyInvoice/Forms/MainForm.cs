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
        private List<CustomEntry> _customEntries = new();
        private DataGridView dgvRecords = null!;
        private DataGridView dgvLineItems = null!;
        private DataGridView dgvCustomEntries = null!;

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
                Enabled = false   // enabled by UpdateGenerateButton()
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
                Text = "Ready. Load a CSV or add custom entries to create an invoice.",
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

            // Top third: raw records
            var pnlRaw = new Panel { Dock = DockStyle.Top, Height = 180, Padding = new Padding(16, 12, 16, 0) };
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

            var splitter1 = new Splitter { Dock = DockStyle.Top, Height = 6, BackColor = Color.FromArgb(220, 225, 235) };

            // Middle: custom entries
            var pnlCustom = new Panel { Dock = DockStyle.Top, Height = 170, Padding = new Padding(16, 8, 16, 0) };
            var lblCustom = new Label
            {
                Text = "CUSTOM ENTRIES",
                Dock = DockStyle.Top,
                Height = 22,
                ForeColor = Color.FromArgb(44, 95, 138),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };

            // Custom entry input row
            var pnlCustomInput = new Panel { Dock = DockStyle.Top, Height = 36 };

            var txtCustomDate = new TextBox
            {
                PlaceholderText = "Date (optional, e.g. 2026-04-01)",
                Location = new Point(0, 6),
                Size = new Size(210, 26),
                Font = new Font("Segoe UI", 9f)
            };

            var txtCustomDesc = new TextBox
            {
                PlaceholderText = "Description *",
                Location = new Point(218, 6),
                Size = new Size(320, 26),
                Font = new Font("Segoe UI", 9f)
            };

            var txtCustomAmount = new TextBox
            {
                PlaceholderText = "Amount * (e.g. 500.00)",
                Location = new Point(546, 6),
                Size = new Size(170, 26),
                Font = new Font("Segoe UI", 9f)
            };

            var btnAddEntry = new Button
            {
                Text = "＋ Add",
                Location = new Point(724, 4),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(44, 138, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddEntry.FlatAppearance.BorderSize = 0;
            btnAddEntry.Click += (s, e) =>
            {
                var descText = txtCustomDesc.Text.Trim();
                if (string.IsNullOrEmpty(descText))
                {
                    MessageBox.Show("Description is required.", "Missing Field", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(txtCustomAmount.Text.Trim(), out var amount) || amount <= 0)
                {
                    MessageBox.Show("Please enter a valid positive amount.", "Invalid Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _customEntries.Add(new CustomEntry
                {
                    Date = txtCustomDate.Text.Trim(),
                    Description = descText,
                    Amount = amount
                });

                txtCustomDate.Clear();
                txtCustomDesc.Clear();
                txtCustomAmount.Clear();
                txtCustomDesc.Focus();

                RefreshCustomEntriesGrid();
                BuildLineItems();
                UpdateGenerateButton();
            };

            var btnRemoveEntry = new Button
            {
                Text = "✕ Remove",
                Location = new Point(812, 4),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(180, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            btnRemoveEntry.FlatAppearance.BorderSize = 0;
            btnRemoveEntry.Click += (s, e) =>
            {
                if (dgvCustomEntries.SelectedRows.Count == 0) return;
                var idx = dgvCustomEntries.SelectedRows[0].Index;
                if (idx >= 0 && idx < _customEntries.Count)
                {
                    _customEntries.RemoveAt(idx);
                    RefreshCustomEntriesGrid();
                    BuildLineItems();
                    UpdateGenerateButton();
                }
            };

            pnlCustomInput.Controls.AddRange(new Control[] { txtCustomDate, txtCustomDesc, txtCustomAmount, btnAddEntry, btnRemoveEntry });

            dgvCustomEntries = CreateGrid();
            dgvCustomEntries.Dock = DockStyle.Fill;

            pnlCustom.Controls.Add(dgvCustomEntries);
            pnlCustom.Controls.Add(pnlCustomInput);
            pnlCustom.Controls.Add(lblCustom);

            var splitter2 = new Splitter { Dock = DockStyle.Top, Height = 6, BackColor = Color.FromArgb(220, 225, 235) };

            // Bottom: invoice line items
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
            pnlSplit.Controls.Add(splitter2);
            pnlSplit.Controls.Add(pnlCustom);
            pnlSplit.Controls.Add(splitter1);
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
                UpdateGenerateButton();
                lblStatus.Text = $"Loaded {_records.Count} records from {Path.GetFileName(dlg.FileName)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading CSV:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateGenerateButton()
        {
            btnGenerateInvoice.Enabled = _records.Count > 0 || _customEntries.Count > 0;
        }

        private void RefreshCustomEntriesGrid()
        {
            dgvCustomEntries.DataSource = null;
            var dt = new System.Data.DataTable();
            dt.Columns.AddRange(new[]
            {
                new System.Data.DataColumn("Date"),
                new System.Data.DataColumn("Description"),
                new System.Data.DataColumn("Amount"),
            });
            foreach (var e in _customEntries)
                dt.Rows.Add(
                    string.IsNullOrEmpty(e.Date) ? "—" : e.Date,
                    e.Description,
                    $"{_profile.CurrencySymbol}{e.Amount:F2}");
            dgvCustomEntries.DataSource = dt;
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

            // Append custom entries as fixed-amount line items
            foreach (var ce in _customEntries)
            {
                lineItems.Add(new InvoiceLineItem
                {
                    Date = ce.Date,
                    Project = "",
                    Description = ce.Description,
                    Hours = 0,
                    Rate = 0,
                    PrecomputedAmount = ce.Amount
                });
            }

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
                            PrecomputedAmount = g.Sum(r => r.BillableAmount)
                        };
                    })
                    .OrderBy(i => i.Date)
                    .ThenBy(i => i.Project)
                    .ToList();

                // Append custom entries
                foreach (var ce in _customEntries)
                {
                    lineItems.Add(new InvoiceLineItem
                    {
                        Date = ce.Date,
                        Project = "",
                        Description = ce.Description,
                        Hours = 0,
                        Rate = 0,
                        PrecomputedAmount = ce.Amount
                    });
                }

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
