using PharmaCare.Data.Models;

namespace PharmaCare.MVC.Models.ViewModels
{
    public class InventoryViewModel
    {
        public List<Inventory> InventoryItems { get; set; } = new();
        
        // Statistics
        public int TotalUniqueItems { get; set; }
        public int TotalQuantity { get; set; }
        public int LowStockCount { get; set; }
        public int ExpiredCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public decimal TotalValue { get; set; }
        
        // Filters
        public string SearchTerm { get; set; } = string.Empty;
        public string CategoryFilter { get; set; } = string.Empty;
        public string StockFilter { get; set; } = string.Empty; // All, LowStock, InStock
        public string ExpiryFilter { get; set; } = string.Empty; // All, Expired, ExpiringSoon
        
        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }

    public class AddInventoryViewModel
    {
        public string MedicineName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int QuantityInStock { get; set; }
        public string Unit { get; set; } = "tablets";
        public decimal Price { get; set; }
        public int ReorderLevel { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Manufacturer { get; set; }
        public string? Category { get; set; }
    }

    public class EditInventoryViewModel
    {
        public int InventoryId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int QuantityInStock { get; set; }
        public string Unit { get; set; } = "tablets";
        public decimal Price { get; set; }
        public int ReorderLevel { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Manufacturer { get; set; }
        public string? Category { get; set; }
    }
}