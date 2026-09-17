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
        private readonly AppDbcontext _context; // تأكدي من اسم الـ DbContext عندك

        public DonationRepo(AppDbcontext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Donation>> GetByDonorIdAsync(Guid donorId)
        {
            return await _context.Donations
                .Include(d => d.BloodBank)
                .Where(d => d.DonorId == donorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Donation>> GetByBankIdAsync(Guid bloodBankId)
        {
            return await _context.Donations
                .Include(d => d.Donor)
                .ThenInclude(d => d.User)
                .Where(d => d.BloodBankId == bloodBankId)
                .ToListAsync();
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
