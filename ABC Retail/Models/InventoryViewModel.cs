namespace ABC_Retail.Models
{
    public class InventoryViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public List<QueueMessageModel> QueueMessages { get; set; } = new List<QueueMessageModel>();
    }
}