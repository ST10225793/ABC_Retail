using ABC_Retail.Models;
using ABC_Retail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class OrdersController : Controller
    {
        private readonly QueueService _queueService;
        private readonly BlobService _blobService;

        public OrdersController(QueueService queueService, BlobService blobService)
        {
            _queueService = queueService;
            _blobService = blobService;
        }

        // GET: View active order queue messages
        public async Task<IActionResult> Index()
        {
            var viewModel = new InventoryViewModel
            {
                Products = await _blobService.GetAllProductsAsync(),
                QueueMessages = await _queueService.PeekMessagesAsync()
            };

            return View(viewModel);
        }

        // POST: Add new order/inventory message to queue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string messageText)
        {
            if (!string.IsNullOrEmpty(messageText))
            {
                await _queueService.SendMessageAsync(messageText);
                TempData["SuccessMessage"] = "Order processing event queued successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Clear queue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            await _queueService.ClearQueueAsync();
            TempData["SuccessMessage"] = "Queue cleared successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}