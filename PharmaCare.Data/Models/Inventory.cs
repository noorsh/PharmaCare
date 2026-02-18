using System.ComponentModel.DataAnnotations;

namespace PharmaCare.Data.Models
{
    public class Inventory
    {
        [Key]
        public int InventoryId { get; set; }

        [Required]
        [MaxLength(200)]
        public string MedicineName { get; set; } 

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public int QuantityInStock { get; set; }

        [Required]
        [MaxLength(50)]
        public string Unit { get; set; } = "tablets"; // tablets, bottles, boxes, ml, etc.

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int ReorderLevel { get; set; } // Alert when stock <= this number

        public DateTime? ExpiryDate { get; set; }

        [MaxLength(100)]
        public string? Manufacturer { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; } // Pain Relief, Antibiotics, Vitamins, etc.

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Computed properties
        public bool IsLowStock => QuantityInStock <= ReorderLevel;
        
        public bool IsExpiringSoon => ExpiryDate.HasValue && 
                                      ExpiryDate.Value <= DateTime.UtcNow.AddMonths(3);
        
        public bool IsExpired => ExpiryDate.HasValue && 
                                 ExpiryDate.Value <= DateTime.UtcNow;
    }
}