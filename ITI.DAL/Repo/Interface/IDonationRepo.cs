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
        Task<bool> DonorExistsAsync(Guid donorId);
        Task<bool> ProcessAsync(Guid donationId, Guid bloodBankId, string fromStatus, string toStatus);
        Task AddAsync(Donation donation);
        Task<bool> SaveChangesAsync();
    }
}
