using System.Net;
using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ABC_Retail_Functions
{
    public class ProcessQueueMessageFunction
    {
        private readonly ILogger<ProcessQueueMessageFunction> _logger;

        public ProcessQueueMessageFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProcessQueueMessageFunction>();
        }

        [Function("ProcessQueueMessage")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "process-queue")] HttpRequestData req)
        {
            _logger.LogInformation("Processing request to post transaction message to Azure Queue Storage.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var payload = JsonSerializer.Deserialize<QueuePayload>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload == null || string.IsNullOrEmpty(payload.MessageText))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid payload. MessageText is required.");
                return badResponse;
            }

            try
            {
                string connectionString = Environment.GetEnvironmentVariable("AzureStorage")
                    ?? "UseDevelopmentStorage=true";
                string queueName = string.IsNullOrEmpty(payload.QueueName) ? "order-processing" : payload.QueueName;

                var queueClient = new QueueClient(connectionString, queueName);
                await queueClient.CreateIfNotExistsAsync();

                // Encode message to Base64 to ensure broad Azure Queue SDK compatibility
                string bytesBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload.MessageText));
                await queueClient.SendMessageAsync(bytesBase64);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"Successfully queued message to '{queueName}'.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message to Azure Queue: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Internal error: {ex.Message}");
                return errorResponse;
            }
        }
    }

    public class QueuePayload
    {
        public string QueueName { get; set; } = "order-processing";
        public string MessageText { get; set; } = string.Empty;
    }
}