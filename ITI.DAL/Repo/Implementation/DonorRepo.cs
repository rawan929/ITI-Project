using ITI.DAL.Context;
using ITI.DAL.Models;
using ITI.DAL.Repo.Implementation;
using ITI.DAL.Repo.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;


namespace ITI.DAL.Repo.Implementation
{
    public class DonorRepo : IDonorRepo
    {
        private readonly AppDbcontext _context;

        public DonorRepo(AppDbcontext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Donor>> GetAllDonorsWithDetailsAsync()
        {
            return await _context.Donors
                .Include(d => d.BloodType)
                .Include(d => d.User)
                .Include(d => d.Donations)
                .ToListAsync();
        }

        public async Task AddDonorAsync(Donor donor)
        {
            await _context.Donors.AddAsync(donor);
            await _context.SaveChangesAsync();
        }

        public async Task<Donor?> GetByUserIdWithDetailsAsync(Guid userId)
        {
            return await _context.Donors
                .Include(d => d.BloodType)
                .Include(d => d.User)
                .Include(d => d.Donations)
                .FirstOrDefaultAsync(d => d.UserId == userId);
        }

        public async Task UpdateDonorAsync(Donor donor)
        {
            _context.Donors.Update(donor);
            await _context.SaveChangesAsync();
        }

        public async Task<List<BloodType>> GetAllBloodTypesAsync()
        {
            return await _context.BloodTypes.ToListAsync();
        }
    }
}
