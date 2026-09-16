using System.Net;
using System.Text.Json;
using Azure.Storage.Files.Shares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ABC_Retail_Functions
{
    public class SendFileToShareFunction
    {
        private readonly ILogger<SendFileToShareFunction> _logger;

        public SendFileToShareFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SendFileToShareFunction>();
        }

        [Function("SendFileToShare")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "send-file")] HttpRequestData req)
        {
            _logger.LogInformation("Processing request to upload file content to Azure Files Share.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var payload = JsonSerializer.Deserialize<FileSharePayload>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload == null || string.IsNullOrEmpty(payload.FileName) || string.IsNullOrEmpty(payload.FileContent))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid payload. FileName and FileContent are required.");
                return badResponse;
            }

            try
            {
                string connectionString = Environment.GetEnvironmentVariable("AzureStorage")
                    ?? "UseDevelopmentStorage=true";
                string shareName = string.IsNullOrEmpty(payload.ShareName) ? "system-logs" : payload.ShareName;

                var shareClient = new ShareClient(connectionString, shareName);
                await shareClient.CreateIfNotExistsAsync();

                var directoryClient = shareClient.GetRootDirectoryClient();
                var fileClient = directoryClient.GetFileClient(payload.FileName);

                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(payload.FileContent);
                using (var stream = new MemoryStream(bytes))
                {
                    await fileClient.CreateAsync(stream.Length);
                    await fileClient.UploadAsync(stream);
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"Successfully saved file '{payload.FileName}' to Azure Files share '{shareName}'.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error writing to Azure Files Share: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Internal error: {ex.Message}");
                return errorResponse;
            }
        }
    }

    public class FileSharePayload
    {
        public string ShareName { get; set; } = "system-logs";
        public string FileName { get; set; } = string.Empty;
        public string FileContent { get; set; } = string.Empty;
    }
}