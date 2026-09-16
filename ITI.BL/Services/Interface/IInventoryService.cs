using ITI.BLL.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.Services.Interface
{
    public interface IInventoryService
    {
        Task<InventoryVM> GetInventoryVMAsync(Guid BloodBankId);
        Task<bool> UpdateStockAsync(Guid BloodBankId, int BloodTypeId, int UnitsToAdd);
    }
}
