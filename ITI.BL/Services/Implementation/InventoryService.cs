using ITI.BLL.Constants;
using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;

namespace ITI.BLL.Services.Implementation
{
    public class InventoryService : IInventoryService
    {
        private readonly IBloodBankRepo _bloodBankRepo;
        private readonly IBloodInventoryRepo _bloodInventoryRepo;

        public InventoryService(IBloodBankRepo bloodBankRepo, IBloodInventoryRepo bloodInventoryRepo)
        {
            _bloodBankRepo = bloodBankRepo;
            _bloodInventoryRepo = bloodInventoryRepo;
        }

        public async Task<InventoryVM> GetInventoryVMAsync(Guid bloodBankId)
        {
            var bank = await _bloodBankRepo.GetByIdAsync(bloodBankId);
            if (bank == null) return new InventoryVM();

            var bloodTypes = await _bloodInventoryRepo.GetAllBloodTypesAsync();
            var records = await _bloodInventoryRepo.GetByBankIdAsync(bloodBankId);
            var unitsByType = records
                .GroupBy(r => r.BloodTypeId)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.UnitsAvailable));

            var items = bloodTypes.Select(type =>
            {
                unitsByType.TryGetValue(type.Id, out var units);
                var capacity = InventoryRules.CapacityFor(type.Name);
                var percent = capacity > 0 ? (int)Math.Round(units * 100.0 / capacity) : 0;

                return new InventoryItemVM
                {
                    BloodTypeId = type.Id,
                    BloodTypeName = type.Name,
                    UnitsAvailable = units,
                    Capacity = capacity,
                    Percent = percent,
                    Level = InventoryRules.LevelFor(percent)
                };
            }).ToList();

            return new InventoryVM
            {
                BloodBankId = bank.Id,
                BloodBankName = bank.Name,
                Items = items
            };
        }

        public async Task<bool> UpdateStockAsync(Guid bloodBankId, int bloodTypeId, int unitsToAdd)
        {
            var stock = await _bloodInventoryRepo.GetByBankAndTypeAsync(bloodBankId, bloodTypeId);

            if (stock == null)
            {
                var newStock = new BloodInventory
                {
                    Id = Guid.NewGuid(),
                    BloodBankId = bloodBankId,
                    BloodTypeId = bloodTypeId,
                    UnitsAvailable = unitsToAdd > 0 ? unitsToAdd : 0
                };
                await _bloodInventoryRepo.AddAsync(newStock);
            }
            else
            {
                stock.UnitsAvailable += unitsToAdd;
                if (stock.UnitsAvailable < 0) stock.UnitsAvailable = 0;

                _bloodInventoryRepo.Update(stock);
            }

            return await _bloodInventoryRepo.SaveChangesAsync();
        }
    }
}