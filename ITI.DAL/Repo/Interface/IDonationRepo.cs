using ITI.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Repo.Interface
{
    public interface IDonationRepo
    {
        Task<IEnumerable<Donation>> GetByDonorIdAsync(Guid donorId);
        Task<IEnumerable<Donation>> GetByBankIdAsync(Guid bloodBankId);
        Task AddAsync(Donation donation);
        Task<bool> SaveChangesAsync();
    }
}
