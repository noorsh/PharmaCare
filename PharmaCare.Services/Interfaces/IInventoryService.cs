using PharmaCare.Data.Models;

namespace PharmaCare.Business.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<Inventory>> GetAllInventoryAsync();
        Task<Inventory?> GetInventoryByIdAsync(int id);
        Task<Inventory?> GetByNameAsync(string medicineName);
        Task<IEnumerable<Inventory>> SearchInventoryAsync(string searchTerm);
        Task<IEnumerable<Inventory>> GetLowStockItemsAsync();
        Task<IEnumerable<Inventory>> GetExpiredItemsAsync();
        Task<IEnumerable<Inventory>> GetExpiringSoonItemsAsync();
        Task AddInventoryAsync(Inventory inventory);
        Task UpdateInventoryAsync(Inventory inventory);
        Task DeleteInventoryAsync(int id);
        Task<int> GetTotalItemsCountAsync();
        Task<int> GetLowStockCountAsync();
        Task<int> GetExpiredCountAsync();
        Task<decimal> GetTotalInventoryValueAsync();
    }
}