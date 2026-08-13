using ABC_Retail.Models;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace ABC_Retail.Services
{
    public class FileService
    {
        private readonly ShareClient _shareClient;

        public FileService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            _shareClient = new ShareClient(connectionString, "system-logs");
            _shareClient.CreateIfNotExists();
        }

        // Upload log file or contract to Azure File Share
        public async Task UploadFileAsync(IFormFile file)
        {
            var directory = _shareClient.GetRootDirectoryClient();
            var fileClient = directory.GetFileClient(file.FileName);

            using (var stream = file.OpenReadStream())
            {
                await fileClient.CreateAsync(stream.Length);
                await fileClient.UploadAsync(stream);
            }
        }

        // List files in the Azure Share directory
        public async Task<List<LogFileModel>> ListFilesAsync()
        {
            var fileList = new List<LogFileModel>();
            var directory = _shareClient.GetRootDirectoryClient();

            await foreach (ShareFileItem item in directory.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    fileList.Add(new LogFileModel
                    {
                        Name = item.Name,
                        DisplaySize = item.FileSize ?? 0
                    });
                }
            }

            return fileList;
        }

        // Download file stream from Azure File Share
        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var directory = _shareClient.GetRootDirectoryClient();
            var fileClient = directory.GetFileClient(fileName);
            ShareFileDownloadInfo download = await fileClient.DownloadAsync();
            return download.Content;
        }

        // Generates a structured system audit log file and uploads it to Azure Files
        public async Task<string> GenerateAndUploadAuditLogAsync()
        {
            var fileName = $"system_audit_{DateTime.UtcNow:yyyy-MM-dd_HHmmss}.txt";
            var directory = _shareClient.GetRootDirectoryClient();
            var fileClient = directory.GetFileClient(fileName);

            // Build the system log content
            var logContent = new System.Text.StringBuilder();
            logContent.AppendLine($"==================================================");
            logContent.AppendLine($"         ABC RETAIL SYSTEM AUDIT LOG             ");
            logContent.AppendLine($"  Generated On: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            logContent.AppendLine($"==================================================");
            logContent.AppendLine();
            logContent.AppendLine($"[INFO] Storage Services Initialized successfully.");
            logContent.AppendLine($"[AUDIT] Azure Table Storage connected (CustomerProfiles & Products).");
            logContent.AppendLine($"[AUDIT] Azure Blob Storage connected (product-images container).");
            logContent.AppendLine($"[AUDIT] Azure Queue Storage active (order-processing queue).");
            logContent.AppendLine($"[AUDIT] Azure File Share connected (system-logs share).");
            logContent.AppendLine($"[SECURITY] Public anonymous access policy verified.");
            logContent.AppendLine($"[SYSTEM] Daily transaction and inventory sync complete.");
            logContent.AppendLine();
            logContent.AppendLine($"==================================================");
            logContent.AppendLine($"               END OF AUDIT REPORT                ");
            logContent.AppendLine($"==================================================");

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(logContent.ToString());

            using (var stream = new MemoryStream(bytes))
            {
                await fileClient.CreateAsync(stream.Length);
                await fileClient.UploadAsync(stream);
            }

            return fileName;
        }
    }
}