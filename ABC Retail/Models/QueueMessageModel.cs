using System.ComponentModel.DataAnnotations;

namespace ABC_Retail.Models
{
    public class QueueMessageModel
    {
        public string MessageId { get; set; } = string.Empty;
        public string PopReceipt { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Order / Inventory Event Message")]
        public string MessageText { get; set; } = string.Empty;

        public DateTimeOffset? InsertedOn { get; set; }
        public DateTimeOffset? NextVisibleOn { get; set; }
    }
}