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

        private static readonly Dictionary<string, HashSet<string>> CompatibilityMap = new()
        {
            ["O-"] = new HashSet<string> { "O-", "O+", "A-", "A+", "B-", "B+", "AB-", "AB+" },
            ["O+"] = new HashSet<string> { "O+", "A+", "B+", "AB+" },
            ["A-"] = new HashSet<string> { "A-", "A+", "AB-", "AB+" },
            ["A+"] = new HashSet<string> { "A+", "AB+" },
            ["B-"] = new HashSet<string> { "B-", "B+", "AB-", "AB+" },
            ["B+"] = new HashSet<string> { "B+", "AB+" },
            ["AB-"] = new HashSet<string> { "AB-", "AB+" },
            ["AB+"] = new HashSet<string> { "AB+" },
        };

        public DonationRequestService(AppDbcontext context)
        {
            _context = context;
        }

        private static bool IsCompatible(string donorBloodType, string requestedBloodType)
        {
            return CompatibilityMap.TryGetValue(donorBloodType, out var compatibleTypes)
                   && compatibleTypes.Contains(requestedBloodType);
        }

        public async Task<DonationResponseResult> RespondToRequestAsync(Guid donorId, Guid requestId)
        {
            var donor = await _context.Donors
                .Include(d => d.BloodType)
                .FirstOrDefaultAsync(d => d.Id == donorId);

            var request = await _context.BloodRequests
                .Include(r => r.BloodType)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (donor?.BloodType == null || request?.BloodType == null)
            {
                return DonationResponseResult.NotFound;
            }

            if (!IsCompatible(donor.BloodType.Name, request.BloodType.Name))
            {
                return DonationResponseResult.IncompatibleBloodType;
            }

            var existingResponse = await _context.DonationRequests
                .FirstOrDefaultAsync(x =>
                    x.DonorId == donorId &&
                    x.BloodRequestId == requestId);

            if (existingResponse != null)
            {
                return DonationResponseResult.AlreadyResponded;
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

            return DonationResponseResult.Success;
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

        public async Task<List<DonorResponseHistoryVM>> GetDonorResponseHistoryAsync(Guid donorId)
        {
            var history = await _context.DonationRequests
                .Where(dr => dr.DonorId == donorId)
                .Include(dr => dr.BloodRequest)
                    .ThenInclude(br => br.Hospital)
                .Include(dr => dr.BloodRequest)
                    .ThenInclude(br => br.BloodType)
                .OrderByDescending(dr => dr.Id)
                .Select(dr => new DonorResponseHistoryVM
                {
                    RequestId = dr.BloodRequestId,
                    HospitalName = dr.BloodRequest.Hospital.Name,
                    City = dr.BloodRequest.Hospital.City,
                    BloodType = dr.BloodRequest.BloodType.Name,
                    UnitsRequired = dr.BloodRequest.UnitsRequired,
                    Urgency = dr.BloodRequest.Urgency,
                    RequestStatus = dr.BloodRequest.Status,
                    ResponseStatus = dr.Status
                })
                .ToListAsync();

            return history;
        }
    }
}