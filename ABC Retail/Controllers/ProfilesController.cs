using ABC_Retail.Models;
using ABC_Retail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class ProfilesController : Controller
    {
        private readonly TableService _tableService;

        public ProfilesController(TableService tableService)
        {
            _tableService = tableService;
        }

        // GET: Fetch all customer profiles for the main table view
        public async Task<IActionResult> Index()
        {
            var profiles = await _tableService.GetAllCustomersAsync();
            return View(profiles);
        }

        // GET: Fetch existing customer details for editing
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var profile = await _tableService.GetCustomerAsync("Customer", id);
            if (profile == null)
            {
                return NotFound();
            }

            return View(profile);
        }

        // POST: Save new customer profile into Azure Table Storage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerProfile profile)
        {
            if (ModelState.IsValid)
            {
                await _tableService.AddCustomerAsync(profile);
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
            TempData["SuccessMessage"] = "Customer profile deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}