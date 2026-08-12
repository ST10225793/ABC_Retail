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
    }
}