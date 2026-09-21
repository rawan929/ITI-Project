using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Context;
using ITI.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITI.BLL.Services.Implementation
{
    public class BloodRequestService : IBloodRequestService
    {
        private readonly AppDbcontext _context;
        private readonly IDonorMatchingService _matchingService;
        private readonly INotificationService _notificationService;

        public BloodRequestService(
            AppDbcontext context,
            IDonorMatchingService matchingService,
            INotificationService notificationService)
        {
            _context = context;
            _matchingService = matchingService;
            _notificationService = notificationService;
        }

        public async Task<CreateRequestOutcome> CreateRequestAsync(
            CreateBloodRequestViewModel model,
            Guid hospitalId)
        {
            var hospital = await _context.Hospitals
                .FirstOrDefaultAsync(h => h.Id == hospitalId);

            if (hospital == null)
            {
                return new CreateRequestOutcome { Result = CreateRequestResult.HospitalNotFound };
            }

            if (!hospital.IsApproved)
            {
                return new CreateRequestOutcome { Result = CreateRequestResult.HospitalNotApproved };
            }

            var request = new BloodRequest
            {
                Id = Guid.NewGuid(),
                HospitalId = hospitalId,
                BloodTypeId = model.BloodTypeId,
                UnitsRequired = model.UnitsRequired,
                Urgency = model.Urgency,
                Status = "Pending"
            };

            _context.BloodRequests.Add(request);

            await _context.SaveChangesAsync();

            // The request exists now, so find donors and invite them. This is the
            // step that turns a stored row into something donors actually see.
            var matches = await _matchingService.FindMatchingDonorsAsync(request.Id);
            var notified = 0;

            if (matches != null)
            {
                notified = await _notificationService
                    .NotifyMatchingDonorsAsync(request.Id, matches.Donors);
            }

            return new CreateRequestOutcome
            {
                Result = CreateRequestResult.Success,
                BloodRequestId = request.Id,
                MatchedDonors = matches?.Donors.Count ?? 0,
                NotifiedDonors = notified
            };
        }

        public async Task<int> MatchAndNotifyAsync(Guid bloodRequestId)
        {
            var matches = await _matchingService.FindMatchingDonorsAsync(bloodRequestId);
            if (matches == null) return 0;

            return await _notificationService
                .NotifyMatchingDonorsAsync(bloodRequestId, matches.Donors);
        }

        public async Task<MatchingResultVM?> PreviewMatchesAsync(Guid bloodRequestId)
        {
            return await _matchingService.FindMatchingDonorsAsync(bloodRequestId);
        }

        public async Task<IEnumerable<BloodRequest>> GetActiveRequestsAsync(
            string city,
            int? bloodTypeId)
        {
            var query = _context.BloodRequests
                .Include(r => r.Hospital)
                .Include(r => r.BloodType)
                .Where(r => r.Status == "Pending");

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(r => r.Hospital.City == city);
            }

            if (bloodTypeId.HasValue)
            {
                query = query.Where(r => r.BloodTypeId == bloodTypeId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<bool> CloseRequestAsync(Guid requestId)
        {
            var request = await _context.BloodRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return false;

            request.Status = "Fulfilled";

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
