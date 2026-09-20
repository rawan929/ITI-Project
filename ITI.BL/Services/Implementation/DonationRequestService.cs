using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Context;
using ITI.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.Services.Implementation
{
    public class DonationRequestService : IDonationRequestService
    {
        private readonly AppDbcontext _context;

        public DonationRequestService(AppDbcontext context)
        {
            _context = context;
        }

        public async Task<bool> RespondToRequestAsync(Guid donorId, Guid requestId)
        {
            var existingResponse = await _context.DonationRequests
                .FirstOrDefaultAsync(x =>
                    x.DonorId == donorId &&
                    x.BloodRequestId == requestId);

            if (existingResponse != null)
            {
                return false;
            }

            var response = new DonationRequest
            {
                Id = Guid.NewGuid(),
                DonorId = donorId,
                BloodRequestId = requestId,
                Status = "Pending"
            };

            await _context.DonationRequests.AddAsync(response);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<DonorResponseViewModel>> GetResponsesForRequestAsync(Guid requestId)
        {
            var responses = await _context.DonationRequests
           .Where(x => x.BloodRequestId == requestId)
           .Include(x => x.Donor)
               .ThenInclude(x => x.User)
           .Include(x => x.Donor)
               .ThenInclude(x => x.BloodType)
           .Select(x => new DonorResponseViewModel
           {
               DonorId = x.Donor.Id,
               FullName = x.Donor.User.FullName,
               BloodType = x.Donor.BloodType.Name,
               City = x.Donor.User.City,
               PhoneNumber = x.Donor.User.PhoneNumber ?? string.Empty,
               TotalDonations = x.Donor.Donations.Count,
               IsActive = x.Donor.User.IsActive
           })
           .ToListAsync();

            return responses;
        }
    }
}

