namespace ABC_Retail.Models
{
    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalQueueMessages { get; set; }
        public int TotalLogFiles { get; set; }
        public List<QueueMessageModel> RecentQueueEvents { get; set; } = new List<QueueMessageModel>();
    }
}