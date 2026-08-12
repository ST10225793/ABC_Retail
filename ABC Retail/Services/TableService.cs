using ABC_Retail.Models;
using Azure.Data.Tables;

namespace ABC_Retail.Services
{
    public class TableService
    {
        private readonly TableClient _tableClient;

        public TableService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            var serviceClient = new TableServiceClient(connectionString);

            // Automatically creates the table "CustomerProfiles" in Azure if it doesn't exist
            _tableClient = serviceClient.GetTableClient("CustomerProfiles");
            _tableClient.CreateIfNotExists();
        }

        // Add a new customer entity to Azure Table Storage
        public async Task AddCustomerAsync(CustomerProfile profile)
        {
            profile.PartitionKey = "Customer";
            profile.RowKey = Guid.NewGuid().ToString("N"); // Unique key per row
            await _tableClient.AddEntityAsync(profile);
        }

        // Retrieve all customer entities from Azure Table Storage
        public async Task<List<CustomerProfile>> GetAllCustomersAsync()
        {
            var customers = new List<CustomerProfile>();
            var queryResults = _tableClient.QueryAsync<CustomerProfile>(filter: $"PartitionKey eq 'Customer'");

            await foreach (var customer in queryResults)
            {
                customers.Add(customer);
            }

            return customers;
        }
    }
}
