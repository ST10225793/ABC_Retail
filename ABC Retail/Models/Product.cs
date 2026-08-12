using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ABC_Retail.Models
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = Guid.NewGuid().ToString("N");
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100000.00)]
        [DataType(DataType.Currency)]
        public double Price { get; set; }

        [Required]
        [Range(0, 10000)]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; } = 10;

        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
