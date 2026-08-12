using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ABC_Retail.Models
{
    public class CustomerProfile
    {
        // Azure Table mandatory properties
        public string PartitionKey { get; set; } = "Customer"; // Grouping key
        public string RowKey { get; set; } // Unique Identifier (e.g., Email or ID)

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Customer Specific Attributes
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Shipping Address")]
        public string Address { get; set; } = string.Empty;
    }
}
