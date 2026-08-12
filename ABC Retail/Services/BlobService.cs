using ABC_Retail.Models;
using Azure;
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

            // Set the HTTP headers so the browser recognizes it as an image
            var blobHttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
            {
                ContentType = file.ContentType // e.g., "image/jpeg" or "image/png"
            };

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });
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

        public async Task DeleteProductAsync(string partitionKey, string rowKey, string imageUrl)
        {
            // 1. Delete entity record from Azure Table Storage
            await _productTableClient.DeleteEntityAsync(partitionKey, rowKey);

            // 2. Delete corresponding image file from Azure Blob Storage
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var uri = new Uri(imageUrl);
                var fileName = Path.GetFileName(uri.LocalPath);
                var blobClient = _blobContainerClient.GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync();
            }
        }

        // Retrieve a single product by PartitionKey and RowKey
        public async Task<Product?> GetProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _productTableClient.GetEntityAsync<Product>(partitionKey, rowKey);
                return response.Value;
            }
            catch (Azure.RequestFailedException)
            {
                return null;
            }
        }

        // Update existing product metadata and optionally replace its image
        public async Task UpdateProductAsync(Product product, IFormFile? newImageFile)
        {
            product.PartitionKey = "Product";

            if (newImageFile != null && newImageFile.Length > 0)
            {
                // 1. Delete old image if it exists
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    try
                    {
                        var uri = new Uri(product.ImageUrl);
                        var oldFileName = Path.GetFileName(uri.LocalPath);
                        var oldBlobClient = _blobContainerClient.GetBlobClient(oldFileName);
                        await oldBlobClient.DeleteIfExistsAsync();
                    }
                    catch
                    {
                        // Ignore invalid URI format issues
                    }
                }

                // 2. Upload new image
                string newImageUrl = await UploadImageAsync(newImageFile);
                product.ImageUrl = newImageUrl;
            }

            // 3. Update entity using ETag.All to bypass empty ETag concurrency checks
            await _productTableClient.UpdateEntityAsync(product, ETag.All, TableUpdateMode.Replace);
        }
    }
}
