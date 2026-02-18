using Microsoft.EntityFrameworkCore;
using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Implementations;
using PharmaCare.Data.Repositories.Interfaces;

namespace PharmaCare.Data.Repositories
{
    public class InventoryRepository : Repository<Inventory>, IInventoryRepository
    {
        public InventoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Inventory>> GetLowStockItemsAsync()
        {
            return await _context.Inventories
                .Where(i => i.IsActive && i.QuantityInStock <= i.ReorderLevel)
                .OrderBy(i => i.QuantityInStock)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetExpiredItemsAsync()
        {
            return await _context.Inventories
                .Where(i => i.IsActive && i.ExpiryDate.HasValue && i.ExpiryDate.Value <= DateTime.UtcNow)
                .OrderBy(i => i.ExpiryDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetExpiringSoonItemsAsync()
        {
            var threeMonthsFromNow = DateTime.UtcNow.AddMonths(3);
            return await _context.Inventories
                .Where(i => i.IsActive && 
                           i.ExpiryDate.HasValue && 
                           i.ExpiryDate.Value > DateTime.UtcNow && 
                           i.ExpiryDate.Value <= threeMonthsFromNow)
                .OrderBy(i => i.ExpiryDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> SearchByNameAsync(string searchTerm)
        {
            return await _context.Inventories
                .Where(i => i.IsActive && i.MedicineName.Contains(searchTerm))
                .OrderBy(i => i.MedicineName)
                .ToListAsync();
        }

        public async Task<Inventory?> GetByNameAsync(string medicineName)
        {
            return await _context.Inventories
                .FirstOrDefaultAsync(i => i.MedicineName.ToLower() == medicineName.ToLower());
        }
    }
}