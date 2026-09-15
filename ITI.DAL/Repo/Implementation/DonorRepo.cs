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
    }
}
