using ClockifyInvoice.Models;
using ClockifyInvoice.Services;

namespace ClockifyInvoice.Forms
{
    public class ProfileEditorForm : Form
    {
        private readonly ProfileService _profileService;
        private InvoiceProfile _profile;
        private TabControl tabControl = null!;
        private Button btnSave = null!, btnCancel = null!;

        // Freelancer fields
        private TextBox txtFreelancerName = null!, txtFreelancerEmail = null!, txtFreelancerPhone = null!;
        private TextBox txtFreelancerAddress = null!, txtFreelancerCity = null!, txtFreelancerWebsite = null!;

        // Client fields
        private TextBox txtClientName = null!, txtClientEmail = null!, txtClientAddress = null!, txtClientCity = null!;

        // Invoice settings
        private TextBox txtPrefix = null!, txtCurrency = null!, txtCurrencySymbol = null!;
        private TextBox txtPaymentNotes = null!, txtTaxLabel = null!, txtAccentColor = null!;
        private NumericUpDown numNextInvoice = null!, numPaymentTerms = null!, numTaxRate = null!, numHourlyRate = null!;

        public ProfileEditorForm(ProfileService profileService)
        {
            _profileService = profileService;
            _profile = profileService.Load();
            InitializeComponent();
            PopulateFields();
        }

        private void InitializeComponent()
        {
            Text = "Invoice Profile Settings";
            Size = new Size(600, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(250, 251, 253);
            Font = new Font("Segoe UI", 9f);

            tabControl = new TabControl { Dock = DockStyle.None, Location = new Point(16, 16), Size = new Size(560, 430) };

            tabControl.TabPages.Add(BuildFreelancerTab());
            tabControl.TabPages.Add(BuildClientTab());
            tabControl.TabPages.Add(BuildInvoiceTab());

            btnSave = new Button
            {
                Text = "Save Profile",
                Location = new Point(390, 460),
                Size = new Size(120, 36),
                BackColor = Color.FromArgb(44, 95, 138),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(260, 460),
                Size = new Size(120, 36),
                BackColor = Color.FromArgb(200, 205, 215),
                ForeColor = Color.FromArgb(50, 55, 65),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] { tabControl, btnSave, btnCancel });
        }

        private TabPage BuildFreelancerTab()
        {
            var tab = new TabPage("  My Info  ");
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
            tab.Controls.Add(panel);

            int y = 16;
            txtFreelancerName = AddField(panel, "Full Name / Company Name", ref y);
            txtFreelancerEmail = AddField(panel, "Email", ref y);
            txtFreelancerPhone = AddField(panel, "Phone", ref y);
            txtFreelancerAddress = AddField(panel, "Address", ref y);
            txtFreelancerCity = AddField(panel, "City, State, ZIP", ref y);
            txtFreelancerWebsite = AddField(panel, "Website", ref y);
            return tab;
        }

        private TabPage BuildClientTab()
        {
            var tab = new TabPage("  Client  ");
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
            tab.Controls.Add(panel);

            int y = 16;
            txtClientName = AddField(panel, "Client / Company Name", ref y);
            txtClientEmail = AddField(panel, "Client Email", ref y);
            txtClientAddress = AddField(panel, "Client Address", ref y);
            txtClientCity = AddField(panel, "Client City, State, ZIP", ref y);
            return tab;
        }

        private TabPage BuildInvoiceTab()
        {
            var tab = new TabPage("  Invoice Settings  ");
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16), AutoScroll = true };
            tab.Controls.Add(panel);

            int y = 16;
            txtPrefix = AddField(panel, "Invoice Number Prefix (e.g. INV)", ref y);
            numNextInvoice = AddNumericField(panel, "Next Invoice Number", ref y, 1, 99999, 0);
            numHourlyRate = AddNumericField(panel, "Default Hourly Rate", ref y, 0, 9999, 2);
            numPaymentTerms = AddNumericField(panel, "Payment Terms (days)", ref y, 0, 365, 0);
            txtCurrency = AddField(panel, "Currency Code (e.g. USD)", ref y);
            txtCurrencySymbol = AddField(panel, "Currency Symbol (e.g. $)", ref y);
            txtTaxLabel = AddField(panel, "Tax Label (e.g. VAT, GST, Tax)", ref y);
            numTaxRate = AddNumericField(panel, "Tax Rate %", ref y, 0, 100, 2);
            txtAccentColor = AddField(panel, "Accent Color (hex, e.g. #2C5F8A)", ref y);
            txtPaymentNotes = AddMultilineField(panel, "Payment Notes", ref y);
            return tab;
        }

        private TextBox AddField(Panel panel, string label, ref int y)
        {
            panel.Controls.Add(new Label
            {
                Text = label,
                Location = new Point(0, y),
                Size = new Size(500, 18),
                ForeColor = Color.FromArgb(80, 90, 110),
                Font = new Font("Segoe UI", 8.5f)
            });
            y += 20;
            var tb = new TextBox
            {
                Location = new Point(0, y),
                Size = new Size(500, 26),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5f)
            };
            panel.Controls.Add(tb);
            y += 38;
            return tb;
        }

        private TextBox AddMultilineField(Panel panel, string label, ref int y)
        {
            panel.Controls.Add(new Label
            {
                Text = label,
                Location = new Point(0, y),
                Size = new Size(500, 18),
                ForeColor = Color.FromArgb(80, 90, 110),
                Font = new Font("Segoe UI", 8.5f)
            });
            y += 20;
            var tb = new TextBox
            {
                Location = new Point(0, y),
                Size = new Size(500, 60),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5f),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            panel.Controls.Add(tb);
            y += 72;
            return tb;
        }

        private NumericUpDown AddNumericField(Panel panel, string label, ref int y, decimal min, decimal max, int decimals)
        {
            panel.Controls.Add(new Label
            {
                Text = label,
                Location = new Point(0, y),
                Size = new Size(500, 18),
                ForeColor = Color.FromArgb(80, 90, 110),
                Font = new Font("Segoe UI", 8.5f)
            });
            y += 20;
            var nud = new NumericUpDown
            {
                Location = new Point(0, y),
                Size = new Size(200, 26),
                Minimum = min,
                Maximum = max,
                DecimalPlaces = decimals,
                Font = new Font("Segoe UI", 9.5f)
            };
            panel.Controls.Add(nud);
            y += 38;
            return nud;
        }

        private void PopulateFields()
        {
            txtFreelancerName.Text = _profile.FreelancerName;
            txtFreelancerEmail.Text = _profile.FreelancerEmail;
            txtFreelancerPhone.Text = _profile.FreelancerPhone;
            txtFreelancerAddress.Text = _profile.FreelancerAddress;
            txtFreelancerCity.Text = _profile.FreelancerCity;
            txtFreelancerWebsite.Text = _profile.FreelancerWebsite;

            txtClientName.Text = _profile.ClientName;
            txtClientEmail.Text = _profile.ClientEmail;
            txtClientAddress.Text = _profile.ClientAddress;
            txtClientCity.Text = _profile.ClientCity;

            txtPrefix.Text = _profile.InvoicePrefix;
            numNextInvoice.Value = _profile.NextInvoiceNumber;
            numHourlyRate.Value = _profile.DefaultHourlyRate;
            numPaymentTerms.Value = _profile.PaymentTermsDays;
            txtCurrency.Text = _profile.Currency;
            txtCurrencySymbol.Text = _profile.CurrencySymbol;
            txtTaxLabel.Text = _profile.TaxLabel;
            numTaxRate.Value = _profile.TaxRate;
            txtAccentColor.Text = _profile.AccentColorHex;
            txtPaymentNotes.Text = _profile.PaymentNotes;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            _profile.FreelancerName = txtFreelancerName.Text.Trim();
            _profile.FreelancerEmail = txtFreelancerEmail.Text.Trim();
            _profile.FreelancerPhone = txtFreelancerPhone.Text.Trim();
            _profile.FreelancerAddress = txtFreelancerAddress.Text.Trim();
            _profile.FreelancerCity = txtFreelancerCity.Text.Trim();
            _profile.FreelancerWebsite = txtFreelancerWebsite.Text.Trim();

            _profile.ClientName = txtClientName.Text.Trim();
            _profile.ClientEmail = txtClientEmail.Text.Trim();
            _profile.ClientAddress = txtClientAddress.Text.Trim();
            _profile.ClientCity = txtClientCity.Text.Trim();

            _profile.InvoicePrefix = txtPrefix.Text.Trim();
            _profile.NextInvoiceNumber = (int)numNextInvoice.Value;
            _profile.DefaultHourlyRate = numHourlyRate.Value;
            _profile.PaymentTermsDays = (int)numPaymentTerms.Value;
            _profile.Currency = txtCurrency.Text.Trim();
            _profile.CurrencySymbol = txtCurrencySymbol.Text.Trim();
            _profile.TaxLabel = txtTaxLabel.Text.Trim();
            _profile.TaxRate = numTaxRate.Value;
            _profile.AccentColorHex = txtAccentColor.Text.Trim();
            _profile.PaymentNotes = txtPaymentNotes.Text.Trim();

            _profileService.Save(_profile);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
