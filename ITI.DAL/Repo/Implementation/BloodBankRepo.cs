using ITI.DAL.Context;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;
using System.Linq;

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Repo.Implementation
{
    public class BloodBankRepo : IBloodBankRepo
    {

        private readonly AppDbcontext _context;

        public BloodBankRepo(AppDbcontext context)
        {
            _context = context;
        }

        public async Task<BloodBank?> GetByIdAsync(Guid id)
        {
            return await _context.BloodBanks.FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task UpdateBloodBankAsync(BloodBank bloodBank)
        {
            _context.BloodBanks.Update(bloodBank);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetApprovedCountAsync() => await _context.BloodBanks.CountAsync(b => b.IsApproved);
    }
}

