using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Business.Services.Interfaces;
using PharmaCare.Data.Models;
using PharmaCare.MVC.Models.ViewModels;

namespace PharmaCare.MVC.Controllers
{
    [Authorize(Roles = "Admin,Pharmacist")]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(
            IInventoryService inventoryService,
            ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, string search = "", string category = "", string stock = "", string expiry = "")
        {
            try
            {
                var viewModel = new InventoryViewModel
                {
                    CurrentPage = page,
                    SearchTerm = search,
                    CategoryFilter = category,
                    StockFilter = stock,
                    ExpiryFilter = expiry
                };

                // Get all inventory items
                var allItems = await _inventoryService.GetAllInventoryAsync();
                var filteredItems = allItems.Where(i => i.IsActive).ToList();

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    filteredItems = filteredItems
                        .Where(i => i.MedicineName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                   (i.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                        .ToList();
                }

                // Apply category filter
                if (!string.IsNullOrEmpty(category))
                {
                    filteredItems = filteredItems.Where(i => i.Category == category).ToList();
                }

                // Apply stock filter
                if (stock == "LowStock")
                {
                    filteredItems = filteredItems.Where(i => i.IsLowStock).ToList();
                }
                else if (stock == "InStock")
                {
                    filteredItems = filteredItems.Where(i => !i.IsLowStock).ToList();
                }

                // Apply expiry filter
                if (expiry == "Expired")
                {
                    filteredItems = filteredItems.Where(i => i.IsExpired).ToList();
                }
                else if (expiry == "ExpiringSoon")
                {
                    filteredItems = filteredItems.Where(i => i.IsExpiringSoon && !i.IsExpired).ToList();
                }

                // Calculate statistics
                viewModel.TotalUniqueItems = allItems.Count(i => i.IsActive);
                viewModel.TotalQuantity = await _inventoryService.GetTotalItemsCountAsync();
                viewModel.LowStockCount = await _inventoryService.GetLowStockCountAsync();
                viewModel.ExpiredCount = await _inventoryService.GetExpiredCountAsync();
                viewModel.ExpiringSoonCount = (await _inventoryService.GetExpiringSoonItemsAsync()).Count();
                viewModel.TotalValue = await _inventoryService.GetTotalInventoryValueAsync();

                // Apply pagination
                viewModel.TotalItems = filteredItems.Count;
                viewModel.InventoryItems = filteredItems
                    .OrderBy(i => i.MedicineName)
                    .Skip((page - 1) * viewModel.PageSize)
                    .Take(viewModel.PageSize)
                    .ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inventory page");
                TempData["Error"] = "Error loading inventory. Please try again.";
                return View(new InventoryViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddInventoryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please fill in all required fields.";
                    return RedirectToAction(nameof(Index));
                }

                var inventory = new Inventory
                {
                    MedicineName = model.MedicineName,
                    Description = model.Description,
                    QuantityInStock = model.QuantityInStock,
                    Unit = model.Unit,
                    Price = model.Price,
                    ReorderLevel = model.ReorderLevel,
                    ExpiryDate = model.ExpiryDate,
                    Manufacturer = model.Manufacturer,
                    Category = model.Category,
                    IsActive = true
                };

                await _inventoryService.AddInventoryAsync(inventory);
                
                TempData["Success"] = $"{model.MedicineName} added to inventory successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding inventory item");
                TempData["Error"] = "Error adding item to inventory.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditInventoryViewModel model)
        {
            try
            {
                var inventory = await _inventoryService.GetInventoryByIdAsync(model.InventoryId);
                if (inventory == null)
                {
                    TempData["Error"] = "Item not found.";
                    return RedirectToAction(nameof(Index));
                }

                inventory.MedicineName = model.MedicineName;
                inventory.Description = model.Description;
                inventory.QuantityInStock = model.QuantityInStock;
                inventory.Unit = model.Unit;
                inventory.Price = model.Price;
                inventory.ReorderLevel = model.ReorderLevel;
                inventory.ExpiryDate = model.ExpiryDate;
                inventory.Manufacturer = model.Manufacturer;
                inventory.Category = model.Category;

                await _inventoryService.UpdateInventoryAsync(inventory);
                
                TempData["Success"] = "Item updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing inventory item");
                TempData["Error"] = "Error updating item.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _inventoryService.DeleteInventoryAsync(id);
                TempData["Success"] = "Item deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting inventory item {id}");
                TempData["Error"] = "Error deleting item.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(int id, int quantity)
        {
            try
            {
                var inventory = await _inventoryService.GetInventoryByIdAsync(id);
                if (inventory == null)
                {
                    TempData["Error"] = "Item not found.";
                    return RedirectToAction(nameof(Index));
                }

                inventory.QuantityInStock = quantity;
                await _inventoryService.UpdateInventoryAsync(inventory);
                
                TempData["Success"] = "Stock updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock");
                TempData["Error"] = "Error updating stock.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}