# Clockify Invoice Generator

A Windows Forms application that parses Clockify CSV time reports and generates professional PDF invoices.

## Features

- **Load Clockify CSV exports** — parses all time entries with project, description, hours, and rate
- **Grouped invoice line items** — automatically groups entries by project + description and sums hours
- **Reusable profile** — save your name, address, client info, rates, tax, and branding in a JSON file that persists between sessions
- **Auto-incrementing invoice numbers** — tracks the next invoice number (e.g. `INV-1001`, `INV-1002`, ...)
- **Professional PDF output** — generates a clean, branded invoice with your accent color
- **Tax support** — optional tax rate (VAT, GST, etc.) applied to subtotal

---

## Requirements

- Windows 10 or 11
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (to build from source)

---

## Getting Started

### 1. Build the project

```bash
cd ClockifyInvoice
dotnet build
```

Or open the solution in **Visual Studio 2022** and press **F5** to run.

To publish a standalone `.exe`:
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The output will be in `ClockifyInvoice\bin\Release\net8.0-windows\win-x64\publish\`.

---

### 2. First-time setup

1. Launch the app
2. Click **⚙ Edit Profile**
3. Fill in:
   - **My Info tab** — your name/company, email, phone, address
   - **Client tab** — your client's name and contact info
   - **Invoice Settings tab** — hourly rate, currency, tax rate, payment terms, accent color
4. Click **Save Profile**

Your profile is saved to `%AppData%\ClockifyInvoice\profile.json` and reused on every launch.

---

### 3. Generate an invoice

1. In **Clockify**, export your time report:
   - Go to **Reports → Detailed**
   - Set your date range
   - Click **Export → CSV**
2. In the app, click **📂 Load Clockify CSV** and select your export
3. Review the raw records and grouped line items
4. Set the **Invoice Date** if needed
5. Click **📄 Generate PDF Invoice**
6. Choose where to save the PDF — it will open automatically

---

## Profile JSON Location

`%AppData%\ClockifyInvoice\profile.json`

You can also edit this file manually with any text editor. Example:

```json
{
  "FreelancerName": "....",
  "FreelancerEmail": "....@example.com",
  "FreelancerPhone": "+1 555-0100",
  "FreelancerAddress": "123 Dev Street",
  "FreelancerCity": "Austin, TX 78701",
  "FreelancerWebsite": "www......dev",
  "ClientName": ".... Client",
  "ClientEmail": "billing@.....com",
  "ClientAddress": "456 Corporate Blvd",
  "ClientCity": "Dallas, TX 75201",
  "InvoicePrefix": "INV",
  "NextInvoiceNumber": 1002,
  "Currency": "USD",
  "CurrencySymbol": "$",
  "DefaultHourlyRate": 65.00,
  "PaymentTermsDays": 30,
  "PaymentNotes": "Payment due within 30 days.",
  "TaxLabel": "Tax",
  "TaxRate": 0.0,
  "AccentColorHex": "#2C5F8A"
}
```

---

## Project Structure

```
ClockifyInvoice/
├── ClockifyInvoice.sln
└── ClockifyInvoice/
    ├── ClockifyInvoice.csproj
    ├── Program.cs
    ├── Models/
    │   └── Models.cs              # InvoiceProfile, ClockifyRecord, InvoiceLineItem
    ├── Services/
    │   ├── ClockifyCsvParser.cs   # CSV parsing with CsvHelper
    │   ├── ProfileService.cs      # JSON save/load for profile
    │   └── PdfInvoiceGenerator.cs # PDF generation with iText7
    └── Forms/
        ├── MainForm.cs            # Main application window
        └── ProfileEditorForm.cs   # Tabbed profile editor
```

## Dependencies (NuGet)

| Package | Purpose |
|---|---|
| `CsvHelper 33.x` | Robust CSV parsing |
| `itext7 8.x` | PDF generation |
| `Newtonsoft.Json 13.x` | Profile JSON serialization |
