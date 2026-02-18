using PharmaCare.Data.Models;

namespace PharmaCare.Data.Repositories.Interfaces
{
    public interface IInventoryRepository : IRepository<Inventory>
    {
        Task<IEnumerable<Inventory>> GetLowStockItemsAsync();
        Task<IEnumerable<Inventory>> GetExpiredItemsAsync();
        Task<IEnumerable<Inventory>> GetExpiringSoonItemsAsync();
        Task<IEnumerable<Inventory>> SearchByNameAsync(string searchTerm);
        Task<Inventory?> GetByNameAsync(string medicineName);
    }
}