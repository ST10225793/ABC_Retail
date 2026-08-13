using ABC_Retail.Models;
using ABC_Retail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class ProductsController : Controller
    {
        private readonly BlobService _blobService;
        private readonly QueueService _queueService;
        private readonly AuditLogService _auditLog;
        public ProductsController(BlobService blobService, QueueService queueService, AuditLogService auditLog)
        {
            _blobService = blobService;
            _queueService = queueService;
            _auditLog = auditLog;
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

                // Automated Queue Events
                await _queueService.SendMessageAsync($"Uploading image: {imageFile.FileName}");
                await _queueService.SendMessageAsync($"Inventory updated for product: {product.Name}");

                _auditLog.LogAction("CATALOG", $"Uploaded product '{product.Name}' priced at R{product.Price}");

                TempData["SuccessMessage"] = "Product uploaded and saved!";
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

            // Automated Queue Event
            await _queueService.SendMessageAsync($"Removed product ID: {rowKey} from inventory");

            TempData["SuccessMessage"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var product = await _blobService.GetProductAsync("Product", id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                await _blobService.UpdateProductAsync(product, imageFile);

                if (imageFile != null && imageFile.Length > 0)
                {
                    await _queueService.SendMessageAsync($"Uploading image: {imageFile.FileName}");
                }
                await _queueService.SendMessageAsync($"Updated product details for {product.Name}");

                TempData["SuccessMessage"] = "Product updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustStock(string partitionKey, string rowKey, int changeAmount)
        {
            var product = await _blobService.GetProductAsync(partitionKey, rowKey);
            if (product != null)
            {
                product.StockQuantity += changeAmount;
                if (product.StockQuantity < 0) product.StockQuantity = 0;

                await _blobService.UpdateProductAsync(product, null);

                // Automated Queue Trigger on Quantity Change
                string actionType = changeAmount < 0 ? "DISPATCH / DECREASE" : "RESTOCK / INCREASE";
                string queueMsg = $"STOCK_ADJUSTMENT | Item: {product.Name} | Action: {actionType} ({changeAmount}) | Remaining Stock: {product.StockQuantity}";
                await _queueService.SendMessageAsync(queueMsg);

                _auditLog.LogAction("INVENTORY", $"Stock changed for '{product.Name}'. New balance: {product.StockQuantity} units");

                TempData["SuccessMessage"] = $"Stock updated for {product.Name}! Queue event triggered.";
            }

            return RedirectToAction("Index", "Orders");
        }
    }
}