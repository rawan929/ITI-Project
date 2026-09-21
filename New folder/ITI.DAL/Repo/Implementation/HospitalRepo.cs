using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using ITI.DAL.Context;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;

namespace ITI.DAL.Repo.Implementation
{
    public class HospitalRepo : IHospitalRepo
    {
        private readonly AppDbcontext _context;

        public HospitalRepo(AppDbcontext context)
        {
            _context = context;
        }

        public async Task<List<Hospital>> GetPendingHospitalsAsync()
        {
            return await _context.Hospitals
                .Include(h => h.User)
                .Where(h => !h.IsApproved)
                .ToListAsync();
        }


        public async Task<Hospital?> GetByIdAsync(Guid id)
        {
            return await _context.Hospitals.FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task UpdateHospitalAsync(Hospital hospital)
        {
            _context.Hospitals.Update(hospital);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetApprovedCountAsync() => await _context.Hospitals.CountAsync(h => h.IsApproved);

        public async Task<int> GetPendingCountAsync() => await _context.Hospitals.CountAsync(h => !h.IsApproved);

        public async Task AddHospitalAsync(Hospital hospital)
        {
            await _context.Hospitals.AddAsync(hospital);
            await _context.SaveChangesAsync();
        }
    }
}
