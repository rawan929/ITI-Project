using ITI.DAL.Context;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Repo.Implementation
{
    public class DonationRepo : IDonationRepo
    {
        private readonly AppDbcontext _context; 
        public DonationRepo(AppDbcontext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Donation>> GetByDonorIdAsync(Guid donorId)
        {
            return await _context.Donations
                .Include(d => d.BloodBank)
                .Where(d => d.DonorId == donorId)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Donation>> GetByBankIdAsync(Guid bloodBankId)
        {
            return await _context.Donations
                .AsNoTracking()
                .Include(d => d.Donor).ThenInclude(d => d.User)
                .Include(d => d.Donor).ThenInclude(d => d.BloodType)
                .Where(d => d.BloodBankId == bloodBankId)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();
        }

        public async Task<bool> DonorExistsAsync(Guid donorId)
        {
            return await _context.Donors.AnyAsync(d => d.Id == donorId);
        }

        public async Task<bool> ProcessAsync(Guid donationId, Guid bloodBankId, string fromStatus, string toStatus)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            var donation = await _context.Donations
                .AsNoTracking()
                .Include(d => d.Donor)
                .FirstOrDefaultAsync(d => d.Id == donationId && d.BloodBankId == bloodBankId);

            if (donation == null || donation.Status != fromStatus) return false;
            var claimed = await _context.Donations
                .Where(d => d.Id == donationId && d.Status == fromStatus)
                .ExecuteUpdateAsync(s => s.SetProperty(d => d.Status, toStatus));
            if (claimed == 0) return false;

            var units = donation.Units;
            var typeId = donation.Donor.BloodTypeId;

            var updated = await _context.BloodInventories
                .Where(i => i.BloodBankId == bloodBankId && i.BloodTypeId == typeId)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.UnitsAvailable, i => i.UnitsAvailable + units));

            if (updated == 0)  
            {
                _context.BloodInventories.Add(new BloodInventory
                {
                    Id = Guid.NewGuid(),
                    BloodBankId = bloodBankId,
                    BloodTypeId = typeId,
                    UnitsAvailable = units
                });
                await _context.SaveChangesAsync();
            }
            var donationDate = donation.DonationDate;
            await _context.Donors
                .Where(d => d.Id == donation.DonorId &&
                            (d.LastDonationDate == null || d.LastDonationDate < donationDate))
                .ExecuteUpdateAsync(s => s.SetProperty(d => d.LastDonationDate, donationDate));

            await tx.CommitAsync();
            return true;
        }

        public async Task AddAsync(Donation donation)
        {
            await _context.Donations.AddAsync(donation);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
