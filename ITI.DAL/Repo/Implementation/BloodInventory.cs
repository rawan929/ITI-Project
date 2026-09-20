using ITI.DAL.Context;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Repo.Implementation
{
    public class BloodInventoryRepo : IBloodInventoryRepo
    {
        private readonly AppDbcontext _context; 

        public BloodInventoryRepo(AppDbcontext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BloodInventory>> GetByBankIdAsync(Guid bloodBankId)
        {
            return await _context.BloodInventories
                .Include(i => i.BloodType)
                .Where(i => i.BloodBankId == bloodBankId)
                .ToListAsync();
        }

        public async Task<BloodInventory?> GetByBankAndTypeAsync(Guid bloodBankId, int bloodTypeId)
        {
            return await _context.BloodInventories
                .FirstOrDefaultAsync(i => i.BloodBankId == bloodBankId && i.BloodTypeId == bloodTypeId);
        }

        public async Task<List<BloodType>> GetAllBloodTypesAsync()
        {
            return await _context.BloodTypes
                .AsNoTracking()
                .OrderBy(b => b.Id)
                .ToListAsync();
        }
        public async Task AddAsync(BloodInventory inventory)
        {
            await _context.BloodInventories.AddAsync(inventory);
        }

        public void Update(BloodInventory inventory)
        {
            _context.BloodInventories.Update(inventory);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}