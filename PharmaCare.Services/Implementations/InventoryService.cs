using Microsoft.Extensions.Logging;
using PharmaCare.Business.Services.Interfaces;
using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Interfaces;

namespace PharmaCare.Business.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(
            IInventoryRepository inventoryRepository,
            IUnitOfWork unitOfWork,
            ILogger<InventoryService> logger)
        {
            _inventoryRepository = inventoryRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Inventory>> GetAllInventoryAsync()
        {
            try
            {
                return await _inventoryRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all inventory items");
                throw;
            }
        }

        public async Task<Inventory?> GetInventoryByIdAsync(int id)
        {
            try
            {
                return await _inventoryRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving inventory item with ID {id}");
                throw;
            }
        }

        public async Task<Inventory?> GetByNameAsync(string medicineName)
        {
            try
            {
                return await _inventoryRepository.GetByNameAsync(medicineName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving inventory item by name: {medicineName}");
                throw;
            }
        }

        public async Task<IEnumerable<Inventory>> SearchInventoryAsync(string searchTerm)
        {
            try
            {
                return await _inventoryRepository.SearchByNameAsync(searchTerm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching inventory with term: {searchTerm}");
                throw;
            }
        }

        public async Task<IEnumerable<Inventory>> GetLowStockItemsAsync()
        {
            try
            {
                return await _inventoryRepository.GetLowStockItemsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving low stock items");
                throw;
            }
        }

        public async Task<IEnumerable<Inventory>> GetExpiredItemsAsync()
        {
            try
            {
                return await _inventoryRepository.GetExpiredItemsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expired items");
                throw;
            }
        }

        public async Task<IEnumerable<Inventory>> GetExpiringSoonItemsAsync()
        {
            try
            {
                return await _inventoryRepository.GetExpiringSoonItemsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expiring soon items");
                throw;
            }
        }

        public async Task AddInventoryAsync(Inventory inventory)
        {
            try
            {
                inventory.CreatedAt = DateTime.UtcNow;
                inventory.UpdatedAt = DateTime.UtcNow;
                
                await _inventoryRepository.AddAsync(inventory);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation($"Added inventory item: {inventory.MedicineName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding inventory item: {inventory.MedicineName}");
                throw;
            }
        }

        public async Task UpdateInventoryAsync(Inventory inventory)
        {
            try
            {
                inventory.UpdatedAt = DateTime.UtcNow;
                
                _inventoryRepository.Update(inventory);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation($"Updated inventory item: {inventory.MedicineName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating inventory item: {inventory.MedicineName}");
                throw;
            }
        }

        public async Task DeleteInventoryAsync(int id)
        {
            try
            {
                var inventory = await _inventoryRepository.GetByIdAsync(id);
                if (inventory != null)
                {
                    _inventoryRepository.Remove(inventory);
                    await _unitOfWork.SaveChangesAsync();
                    
                    _logger.LogInformation($"Deleted inventory item: {inventory.MedicineName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting inventory item with ID {id}");
                throw;
            }
        }

        public async Task<int> GetTotalItemsCountAsync()
        {
            try
            {
                var items = await _inventoryRepository.GetAllAsync();
                return items.Where(i => i.IsActive).Sum(i => i.QuantityInStock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total items count");
                throw;
            }
        }

        public async Task<int> GetLowStockCountAsync()
        {
            try
            {
                var items = await _inventoryRepository.GetLowStockItemsAsync();
                return items.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock count");
                throw;
            }
        }

        public async Task<int> GetExpiredCountAsync()
        {
            try
            {
                var items = await _inventoryRepository.GetExpiredItemsAsync();
                return items.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expired count");
                throw;
            }
        }

        public async Task<decimal> GetTotalInventoryValueAsync()
        {
            try
            {
                var items = await _inventoryRepository.GetAllAsync();
                return items.Where(i => i.IsActive).Sum(i => i.Price * i.QuantityInStock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total inventory value");
                throw;
            }
        }
    }
}