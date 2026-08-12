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

        // GET: Fetch real customer records from Azure Table Storage
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
                TempData["SuccessMessage"] = "Customer profile successfully saved to Azure Table Storage!";
                return RedirectToAction(nameof(Index));
            }

            var profiles = await _tableService.GetAllCustomersAsync();
            return View("Index", profiles);
        }
    }
}
