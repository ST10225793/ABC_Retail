using System.Net;
using System.Text.Json;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ABC_Retail_Functions
{
    public class StoreTableDataFunction
    {
        private readonly ILogger<StoreTableDataFunction> _logger;

        public StoreTableDataFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<StoreTableDataFunction>();
        }

        [Function("StoreTableData")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "store-table")] HttpRequestData req)
        {
            _logger.LogInformation("Processing request to store data in Azure Table Storage.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var payload = JsonSerializer.Deserialize<TablePayload>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload == null || string.IsNullOrEmpty(payload.TableName) || string.IsNullOrEmpty(payload.PartitionKey))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid payload. TableName and PartitionKey are required.");
                return badResponse;
            }

            try
            {
                string connectionString = Environment.GetEnvironmentVariable("AzureStorage")
                    ?? "UseDevelopmentStorage=true";

                var serviceClient = new TableServiceClient(connectionString);
                var tableClient = serviceClient.GetTableClient(payload.TableName);
                await tableClient.CreateIfNotExistsAsync();

                var entity = new TableEntity(payload.PartitionKey, payload.RowKey ?? Guid.NewGuid().ToString());

                foreach (var kvp in payload.Data)
                {
                    entity[kvp.Key] = kvp.Value?.ToString();
                }

                await tableClient.UpsertEntityAsync(entity);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"Successfully upserted record into table '{payload.TableName}' with RowKey '{entity.RowKey}'.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error writing to Azure Table: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Internal error: {ex.Message}");
                return errorResponse;
            }
        }
    }

    public class TablePayload
    {
        public string TableName { get; set; } = string.Empty;
        public string PartitionKey { get; set; } = string.Empty;
        public string? RowKey { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
}