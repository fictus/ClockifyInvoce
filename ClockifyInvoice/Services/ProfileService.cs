using Newtonsoft.Json;
using ClockifyInvoice.Models;

namespace ClockifyInvoice.Services
{
    public class ProfileService
    {
        private readonly string _profilePath;

        public ProfileService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "ClockifyInvoice");
            Directory.CreateDirectory(dir);
            _profilePath = Path.Combine(dir, "profile.json");
        }

        public string ProfilePath => _profilePath;

        public InvoiceProfile Load()
        {
            if (!File.Exists(_profilePath))
                return new InvoiceProfile();

            try
            {
                var json = File.ReadAllText(_profilePath);
                return JsonConvert.DeserializeObject<InvoiceProfile>(json) ?? new InvoiceProfile();
            }
            catch
            {
                return new InvoiceProfile();
            }
        }

        public void Save(InvoiceProfile profile)
        {
            var json = JsonConvert.SerializeObject(profile, Formatting.Indented);
            File.WriteAllText(_profilePath, json);
        }

        public void IncrementInvoiceNumber(InvoiceProfile profile)
        {
            profile.NextInvoiceNumber++;
            Save(profile);
        }
    }
}
