using ABC_Retail.Models;
using ABC_Retail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class ProfilesController : Controller
    {
        private readonly TableService _tableService;
        private readonly QueueService _queueService;

        public ProfilesController(TableService tableService, QueueService queueService)
        {
            _tableService = tableService;
            _queueService = queueService;
        }

        // GET: Fetch all customer profiles for the main table view
        public async Task<IActionResult> Index()
        {
            var profiles = await _tableService.GetAllCustomersAsync();
            return View(profiles);
        }


        // POST: Save new customer profile into Azure Table Storage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerProfile profile)
        {
            if (ModelState.IsValid)
            {
                await _tableService.AddCustomerAsync(profile);

                // Automated Queue Event
                string logMsg = $"Processing customer registration: {profile.FirstName} {profile.LastName}";
                await _queueService.SendMessageAsync(logMsg);

                TempData["SuccessMessage"] = "Customer profile successfully saved!";
                return RedirectToAction(nameof(Index));
            }

            var profiles = await _tableService.GetAllCustomersAsync();
            return View("Index", profiles);
        }

        // POST: Save updated customer details back to Azure Table Storage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerProfile profile)
        {
            if (ModelState.IsValid)
            {
                await _tableService.UpdateCustomerAsync(profile);

                // Automated Queue Event
                string logMsg = $"Updated profile details for {profile.FirstName} {profile.LastName}";
                await _queueService.SendMessageAsync(logMsg);

                TempData["SuccessMessage"] = "Customer profile updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(profile);
        }

        // POST: Delete customer profile from Azure Table Storage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            await _tableService.DeleteCustomerAsync(partitionKey, rowKey);

            // Automated Queue Event
            await _queueService.SendMessageAsync($"Deleted customer profile ID: {rowKey}");

            TempData["SuccessMessage"] = "Customer profile deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}