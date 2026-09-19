using ITI.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Repo.Interface
{
    public interface IBloodBankRepo
    {
        Task<BloodBank?> GetByIdAsync(Guid id);
        Task UpdateBloodBankAsync(BloodBank bloodBank);
        Task<int> GetApprovedCountAsync();
    }
}
