using ABC_Retail.Models;
using ABC_Retail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class ProductsController : Controller
    {
        private readonly BlobService _blobService;

        public ProductsController(BlobService blobService)
        {
            _blobService = blobService;
        }

        // GET: Fetch all products and display catalog
        public async Task<IActionResult> Index()
        {
            var products = await _blobService.GetAllProductsAsync();
            return View(products);
        }

        // POST: Upload image to Blob and save metadata to Table
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                // Upload image to Azure Blob Storage and retrieve public URL
                string imageUrl = await _blobService.UploadImageAsync(imageFile);
                product.ImageUrl = imageUrl;

                // Save product record to Azure Table Storage
                await _blobService.AddProductAsync(product);

                TempData["SuccessMessage"] = "Product uploaded and saved to Azure Storage successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Please select a valid image file.");
            var products = await _blobService.GetAllProductsAsync();
            return View("Index", products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey, string imageUrl)
        {
            await _blobService.DeleteProductAsync(partitionKey, rowKey, imageUrl);
            TempData["SuccessMessage"] = "Product and image deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}