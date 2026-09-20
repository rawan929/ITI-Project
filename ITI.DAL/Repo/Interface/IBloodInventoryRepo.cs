using System;
using System.Collections.Generic;
using System.Text;
using ITI.DAL.Models;

namespace ITI.DAL.Repo.Interface
{
    public interface IBloodInventoryRepo
    {
        Task<IEnumerable<BloodInventory>> GetByBankIdAsync(Guid bloodBankId);
        Task<BloodInventory?> GetByBankAndTypeAsync(Guid bloodBankId, int bloodTypeId);
        Task<List<BloodType>> GetAllBloodTypesAsync();
        Task AddAsync(BloodInventory inventory);
        void Update(BloodInventory inventory);
        Task<bool> SaveChangesAsync();
    }
}
