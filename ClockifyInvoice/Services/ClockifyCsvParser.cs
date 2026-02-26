using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using ClockifyInvoice.Models;

namespace ClockifyInvoice.Services
{
    public class ClockifyCsvParser
    {
        public List<ClockifyRecord> Parse(string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null,
                HeaderValidated = null,
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<ClockifyRecordMap>();
            return csv.GetRecords<ClockifyRecord>().ToList();
        }
    }

    public sealed class ClockifyRecordMap : ClassMap<ClockifyRecord>
    {
        public ClockifyRecordMap()
        {
            Map(m => m.Project).Name("Project");
            Map(m => m.Client).Name("Client");
            Map(m => m.Description).Name("Description");
            Map(m => m.Task).Name("Task");
            Map(m => m.User).Name("User");
            Map(m => m.Group).Name("Group");
            Map(m => m.Email).Name("Email");
            Map(m => m.Tags).Name("Tags");
            Map(m => m.Billable).Name("Billable");
            Map(m => m.StartDate).Name("Start Date");
            Map(m => m.StartTime).Name("Start Time");
            Map(m => m.EndDate).Name("End Date");
            Map(m => m.EndTime).Name("End Time");
            Map(m => m.DurationH).Name("Duration (h)");
            Map(m => m.DurationDecimal).Name("Duration (decimal)").TypeConverterOption.NumberStyles(NumberStyles.Any);
            Map(m => m.BillableRate).Name("Billable Rate (USD)").TypeConverterOption.NumberStyles(NumberStyles.Any);
            Map(m => m.BillableAmount).Name("Billable Amount (USD)").TypeConverterOption.NumberStyles(NumberStyles.Any);
            Map(m => m.DateOfCreation).Name("Date of creation");
        }
    }
}
