using System.Net;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ABC_Retail_Functions
{
    public class UploadBlobDataFunction
    {
        private readonly ILogger<UploadBlobDataFunction> _logger;

        public UploadBlobDataFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UploadBlobDataFunction>();
        }

        [Function("UploadBlobData")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "upload-blob")] HttpRequestData req)
        {
            _logger.LogInformation("Processing request to upload image/file to Azure Blob Storage.");

            // Verify multipart form-data content
            if (!req.Headers.TryGetValues("Content-Type", out var contentTypes) ||
                !contentTypes.Any(c => c.Contains("multipart/form-data")))
            {
                // Fallback to base64 JSON payload if raw stream isn't passed via multipart
                return await HandleBase64UploadAsync(req);
            }

            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync("Please submit data using JSON with base64 encoded image content.");
            return response;
        }

        private async Task<HttpResponseData> HandleBase64UploadAsync(HttpRequestData req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var payload = System.Text.Json.JsonSerializer.Deserialize<BlobUploadPayload>(requestBody, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload == null || string.IsNullOrEmpty(payload.ContainerName) || string.IsNullOrEmpty(payload.FileName) || string.IsNullOrEmpty(payload.Base64Data))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid payload. ContainerName, FileName, and Base64Data are required.");
                return badResponse;
            }

            try
            {
                string connectionString = Environment.GetEnvironmentVariable("AzureStorage")
                    ?? "UseDevelopmentStorage=true";

                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(payload.ContainerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var blobClient = containerClient.GetBlobClient(payload.FileName);
                byte[] bytes = Convert.FromBase64String(payload.Base64Data);

                using (var stream = new MemoryStream(bytes))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync(blobClient.Uri.ToString());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading to Azure Blob Storage: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Internal error: {ex.Message}");
                return errorResponse;
            }
        }
    }

    public class BlobUploadPayload
    {
        public string ContainerName { get; set; } = "product-images";
        public string FileName { get; set; } = string.Empty;
        public string Base64Data { get; set; } = string.Empty;
    }
}