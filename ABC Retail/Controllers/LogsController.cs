using ABC_Retail.Models;
using ABC_Retail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class LogsController : Controller
    {
        private readonly FileService _fileService;

        public LogsController(FileService fileService)
        {
            _fileService = fileService;
        }

        // GET: Fetch list of files in Azure Share
        public async Task<IActionResult> Index()
        {
            var files = await _fileService.ListFilesAsync();
            return View(files);
        }

        // POST: Upload log report or file to Azure Files
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile logFile)
        {
            if (logFile != null && logFile.Length > 0)
            {
                await _fileService.UploadFileAsync(logFile);
                TempData["SuccessMessage"] = "File uploaded to Azure File Share successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Please select a valid file to upload.");
            var files = await _fileService.ListFilesAsync();
            return View("Index", files);
        }

        // GET: Download file from Azure Files
        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return NotFound();

            var stream = await _fileService.DownloadFileAsync(fileName);
            return File(stream, "application/octet-stream", fileName);
        }
    }
}