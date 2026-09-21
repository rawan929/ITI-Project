using ITI.DAL.Context;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.DAL.Repo.Implementation
{
    public class DonorMatchingRepo : IDonorMatchingRepo
    {
        private readonly AppDbcontext _context;

        public DonorMatchingRepo(AppDbcontext context)
        {
            _context = context;
        }

        public async Task<BloodRequest?> GetBloodRequestAsync(Guid bloodRequestId)
        {
            return await _context.BloodRequests
                .Include(r => r.Hospital)
                .Include(r => r.BloodType)
                .FirstOrDefaultAsync(r => r.Id == bloodRequestId);
        }

        public async Task<List<Donor>> GetCandidateDonorsAsync(List<int> bloodTypeIds)
        {
            if (bloodTypeIds == null || bloodTypeIds.Count == 0)
            {
                return new List<Donor>();
            }

            return await _context.Donors
                .Include(d => d.User)
                .Include(d => d.BloodType)
                .Include(d => d.Donations)
                .Where(d => d.BloodTypeId != null
                            && bloodTypeIds.Contains(d.BloodTypeId.Value)
                            && d.User.IsActive)
                .ToListAsync();
        }

        public async Task<List<int>> GetBloodTypeIdsByNamesAsync(List<string> names)
        {
            if (names == null || names.Count == 0)
            {
                return new List<int>();
            }

            return await _context.BloodTypes
                .Where(b => names.Contains(b.Name))
                .Select(b => b.Id)
                .ToListAsync();
        }

        public async Task<List<Guid>> GetExistingDonorIdsForRequestAsync(Guid bloodRequestId)
        {
            return await _context.DonationRequests
                .Where(dr => dr.BloodRequestId == bloodRequestId)
                .Select(dr => dr.DonorId)
                .ToListAsync();
        }

        public async Task AddDonationRequestsAsync(List<DonationRequest> donationRequests)
        {
            await _context.DonationRequests.AddRangeAsync(donationRequests);
        }

        public async Task<List<DonationRequest>> GetDonationRequestsForDonorAsync(Guid donorId)
        {
            return await _context.DonationRequests
                .Include(dr => dr.BloodRequest)
                    .ThenInclude(br => br.Hospital)
                .Include(dr => dr.BloodRequest)
                    .ThenInclude(br => br.BloodType)
                .Include(dr => dr.BloodBank)
                .Include(dr => dr.Appointment)
                .Where(dr => dr.DonorId == donorId)
                .ToListAsync();
        }

        public async Task<DonationRequest?> GetDonationRequestAsync(Guid donationRequestId)
        {
            return await _context.DonationRequests
                .Include(dr => dr.BloodRequest)
                    .ThenInclude(br => br.Hospital)
                .Include(dr => dr.BloodRequest)
                    .ThenInclude(br => br.BloodType)
                .Include(dr => dr.BloodBank)
                .FirstOrDefaultAsync(dr => dr.Id == donationRequestId);
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
