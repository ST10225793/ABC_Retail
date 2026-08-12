using ABC_Retail.Models;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class ProfilesController : Controller
    {
        // Temporary sample list representing Azure Table records until Azure Storage service client is injected
        private static List<CustomerProfile> _sampleProfiles = new List<CustomerProfile>
        {
            new CustomerProfile { PartitionKey = "Customer", RowKey = "101", FirstName = "Koketso", LastName = "Masasanya", Email = "koketso@example.com", PhoneNumber = "0821234567", Address = "Centurion, Gauteng" },
            new CustomerProfile { PartitionKey = "Customer", RowKey = "102", FirstName = "Sarah", LastName = "Jenkins", Email = "sarah.j@example.com", PhoneNumber = "0719876543", Address = "Pretoria, Gauteng" },
            new CustomerProfile { PartitionKey = "Customer", RowKey = "103", FirstName = "Lesedi", LastName = "Tau", Email = "lesedi.t@example.com", PhoneNumber = "0835551234", Address = "Johannesburg, Gauteng" },
            new CustomerProfile { PartitionKey = "Customer", RowKey = "104", FirstName = "Michael", LastName = "Brown", Email = "mbrown@example.com", PhoneNumber = "0724449876", Address = "Midrand, Gauteng" },
            new CustomerProfile { PartitionKey = "Customer", RowKey = "105", FirstName = "Amina", LastName = "Patel", Email = "amina.p@example.com", PhoneNumber = "0843332211", Address = "Durban, KZN" }
        };

        public IActionResult Index()
        {
            return View(_sampleProfiles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CustomerProfile profile)
        {
            if (ModelState.IsValid)
            {
                profile.PartitionKey = "Customer";
                profile.RowKey = Guid.NewGuid().ToString("N");
                _sampleProfiles.Add(profile);

                TempData["SuccessMessage"] = "Customer profile saved successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View("Index", _sampleProfiles);
        }
    }
}
