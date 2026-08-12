using ABC_Retail.Models;
using ABC_Retail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class OrdersController : Controller
    {
        private readonly QueueService _queueService;

        public OrdersController(QueueService queueService)
        {
            _queueService = queueService;
        }

        // GET: View active order queue messages
        public async Task<IActionResult> Index()
        {
            var messages = await _queueService.PeekMessagesAsync();
            return View(messages);
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