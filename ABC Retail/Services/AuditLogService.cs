namespace ABC_Retail.Services
{
    public class AuditLogService
    {
        private readonly List<string> _logEntries = new List<string>();

        public AuditLogService()
        {
            // Initial boot entry
            LogAction("SYSTEM", "Storage Services and Audit Logger Initialized.");
        }

        // Record a real activity in the system
        public void LogAction(string category, string message)
        {
            TimeZoneInfo sastTimeZone = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");
            DateTime sastNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, sastTimeZone);

            string entry = $"[{sastNow:yyyy-MM-dd_HHmmss}] [{category.ToUpper()}] {message}";
            _logEntries.Add(entry);
        }

        // Fetch all recorded actions for file export
        public List<string> GetLogs()
        {
            return _logEntries.ToList();
        }
    }
}