using ABC_Retail.Models;
using ABC_Retail.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ABC_Retail.Controllers
{
    public class HomeController : Controller
    {
        private readonly TableService _tableService;
        private readonly BlobService _blobService;
        private readonly QueueService _queueService;
        private readonly FileService _fileService;

        public HomeController(
            TableService tableService,
            BlobService blobService,
            QueueService queueService,
            FileService fileService)
        {
            _tableService = tableService;
            _blobService = blobService;
            _queueService = queueService;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _tableService.GetAllCustomersAsync();
            var products = await _blobService.GetAllProductsAsync();
            var queueMessages = await _queueService.PeekMessagesAsync();
            var logFiles = await _fileService.ListFilesAsync();

            var viewModel = new DashboardViewModel
            {
                TotalCustomers = customers.Count,
                TotalProducts = products.Count,
                TotalQueueMessages = queueMessages.Count,
                TotalLogFiles = logFiles.Count,
                RecentQueueEvents = queueMessages.Take(5).ToList()
            };

            return View(viewModel);
        }

        public IActionResult Profiles() => RedirectToAction("Index", "Profiles");
        public IActionResult Products() => RedirectToAction("Index", "Products");
        public IActionResult Inventory() => RedirectToAction("Index", "Orders");
        public IActionResult Logs() => RedirectToAction("Index", "Logs");
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}