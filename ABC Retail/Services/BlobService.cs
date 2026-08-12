using ABC_Retail.Models;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ABC_Retail.Services
{
    public class BlobService
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly TableClient _productTableClient;

        public BlobService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");

            // Setup Azure Blob Storage Container without forcing public access on creation
            var blobServiceClient = new BlobServiceClient(connectionString);
            _blobContainerClient = blobServiceClient.GetBlobContainerClient("product-images");
            _blobContainerClient.CreateIfNotExists(); // Removed PublicAccessType parameter

            // Setup Azure Table Storage Client for Products
            var tableServiceClient = new TableServiceClient(connectionString);
            _productTableClient = tableServiceClient.GetTableClient("Products");
            _productTableClient.CreateIfNotExists();
        }

        // Upload Product Image to Azure Blob Storage
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var blobClient = _blobContainerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            return blobClient.Uri.ToString();
        }

        // Save Product details to Azure Table Storage
        public async Task AddProductAsync(Product product)
        {
            product.PartitionKey = "Product";
            if (string.IsNullOrEmpty(product.RowKey))
            {
                product.RowKey = Guid.NewGuid().ToString("N");
            }
            await _productTableClient.AddEntityAsync(product);
        }

        // Retrieve all products from Azure Table Storage
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();
            var queryResults = _productTableClient.QueryAsync<Product>(filter: "PartitionKey eq 'Product'");

            await foreach (var product in queryResults)
            {
                products.Add(product);
            }

            return products;
        }
    }
}
