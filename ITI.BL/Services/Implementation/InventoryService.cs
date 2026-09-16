using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.Services.Implementation
{
    public class InventoryService : IInventoryService
    {
        private readonly IBloodBankRepo _bloodBankRepo;
        private readonly IBloodInventoryRepo _bloodInventoryRepo;
        public InventoryService(IBloodBankRepo bloodBankRepo , IBloodInventoryRepo bloodInventoryRepo)
        {
            _bloodBankRepo = bloodBankRepo;
            _bloodInventoryRepo = bloodInventoryRepo;
        }
        public async Task<InventoryVM> GetInventoryVMAsync(Guid bloodBankId)
        {
            var bank = await _bloodBankRepo.GetByIdAsync(bloodBankId);
            if (bank == null) return new InventoryVM();

            var inventoryRecord = await _bloodInventoryRepo.GetByBankIdAsync(bloodBankId);
            var model = new InventoryVM
            {
                BloodBankId = bank.Id,
                BloodBankName = bank.Name,
                invetoryItems = inventoryRecord.Select(i => new InvetoryItemVM
                {
                    BloodTybeId = i.BloodTypeId,
                    BloodTybeName = i.BloodType?.Name ?? string.Empty,
                    UnitsAvailable = i.UnitsAvailable,
                }).ToList()
            };
            return model;
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

