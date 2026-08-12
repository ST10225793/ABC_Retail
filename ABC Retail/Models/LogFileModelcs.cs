namespace ABC_Retail.Models
{
    public class LogFileModel
    {
        public string Name { get; set; } = string.Empty;
        public long DisplaySize { get; set; }
        public DateTimeOffset? LastModified { get; set; }
        public string ShareName { get; set; } = "system-logs";
    }
}