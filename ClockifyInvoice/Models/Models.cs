using Newtonsoft.Json;

namespace ClockifyInvoice.Models
{
    public class InvoiceProfile
    {
        // Sender / Freelancer Info
        public string FreelancerName { get; set; } = "";
        public string FreelancerEmail { get; set; } = "";
        public string FreelancerPhone { get; set; } = "";
        public string FreelancerAddress { get; set; } = "";
        public string FreelancerCity { get; set; } = "";
        public string FreelancerWebsite { get; set; } = "";

        // Client Info
        public string ClientName { get; set; } = "";
        public string ClientEmail { get; set; } = "";
        public string ClientAddress { get; set; } = "";
        public string ClientCity { get; set; } = "";

        // Invoice Settings
        public string InvoicePrefix { get; set; } = "INV";
        public int NextInvoiceNumber { get; set; } = 1001;
        public string Currency { get; set; } = "USD";
        public string CurrencySymbol { get; set; } = "$";
        public decimal DefaultHourlyRate { get; set; } = 65.00m;
        public int PaymentTermsDays { get; set; } = 30;
        public string PaymentNotes { get; set; } = "Payment due within 30 days.";
        public string TaxLabel { get; set; } = "Tax";
        public decimal TaxRate { get; set; } = 0m; // percentage, e.g. 10 for 10%

        // Branding
        public string AccentColorHex { get; set; } = "#2C5F8A";

        [JsonIgnore]
        public string FormattedInvoiceNumber => $"{InvoicePrefix}-{NextInvoiceNumber:D4}";
    }

    public class ClockifyRecord
    {
        public string Project { get; set; } = "";
        public string Client { get; set; } = "";
        public string Description { get; set; } = "";
        public string Task { get; set; } = "";
        public string User { get; set; } = "";
        public string Group { get; set; } = "";
        public string Email { get; set; } = "";
        public string Tags { get; set; } = "";
        public string Billable { get; set; } = "";
        public string StartDate { get; set; } = "";
        public string StartTime { get; set; } = "";
        public string EndDate { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string DurationH { get; set; } = "";
        public decimal DurationDecimal { get; set; }
        public decimal BillableRate { get; set; }
        public decimal BillableAmount { get; set; }
        public string DateOfCreation { get; set; } = "";
    }

    public class InvoiceLineItem
    {
        public string Date { get; set; } = "";
        public string Project { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Hours { get; set; }
        public decimal Rate { get; set; }
        public decimal PrecomputedAmount { get; set; }
        public decimal Amount => PrecomputedAmount > 0 ? PrecomputedAmount : Hours * Rate;
    }
}
