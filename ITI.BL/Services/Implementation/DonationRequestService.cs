using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Context;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.BLL.Services.Implementation
{
    public class DonationRequestService : IDonationRequestService
    {
        private readonly AppDbcontext _context;
        private readonly IDonorMatchingRepo _matchingRepo;

        public DonationRequestService(AppDbcontext context, IDonorMatchingRepo matchingRepo)
        {
            _context = context;
            _matchingRepo = matchingRepo;
        }

        // ------------------------------------------------------------------
        // Donor volunteering for a request they found on the public list
        // ------------------------------------------------------------------

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

            if (!BloodCompatibility.IsCompatible(donor.BloodType.Name, request.BloodType.Name))
            {
                return DonationResponseResult.IncompatibleBloodType;
            }

            var existingResponse = await _context.DonationRequests
                .FirstOrDefaultAsync(x =>
                    x.DonorId == donorId &&
                    x.BloodRequestId == requestId);

            if (existingResponse != null)
            {
                // The donor was already invited by the matching service and is now
                // volunteering from the public list — treat that as accepting.
                if (existingResponse.Status == DonationRequestStatus.Invited)
                {
                    existingResponse.Status = DonationRequestStatus.Accepted;
                    await _context.SaveChangesAsync();
                    return DonationResponseResult.Success;
                }

                return DonationResponseResult.AlreadyResponded;
            }

            var response = new DonationRequest
            {
                Id = Guid.NewGuid(),
                DonorId = donorId,
                BloodRequestId = requestId,
                Status = DonationRequestStatus.Accepted
            };

            await _context.DonationRequests.AddAsync(response);
            await _context.SaveChangesAsync();

            return DonationResponseResult.Success;
        }

        // ------------------------------------------------------------------
        // Donor inbox
        // ------------------------------------------------------------------

        public async Task<List<DonorInboxItemVM>> GetInboxAsync(Guid donorId)
        {
            var rows = await _matchingRepo.GetDonationRequestsForDonorAsync(donorId);

            return rows
                .Select(dr => new DonorInboxItemVM
                {
                    DonationRequestId = dr.Id,
                    BloodRequestId = dr.BloodRequestId,
                    HospitalName = dr.BloodRequest?.Hospital?.Name ?? string.Empty,
                    HospitalCity = dr.BloodRequest?.Hospital?.City ?? string.Empty,
                    HospitalPhone = dr.BloodRequest?.Hospital?.Phone ?? string.Empty,
                    BloodTypeName = dr.BloodRequest?.BloodType?.Name ?? string.Empty,
                    UnitsRequired = dr.BloodRequest?.UnitsRequired ?? 0,
                    Urgency = dr.BloodRequest?.Urgency ?? string.Empty,
                    RequestStatus = dr.BloodRequest?.Status ?? string.Empty,
                    ResponseStatus = dr.Status
                })
                // Things needing an answer first, emergencies above those.
                .OrderByDescending(x => x.IsAwaitingReply)
                .ThenByDescending(x => x.Urgency == "Emergency")
                .ToList();
        }

        public Task<RespondToInvitationResult> AcceptInvitationAsync(Guid donorId, Guid donationRequestId)
            => AnswerInvitationAsync(donorId, donationRequestId, DonationRequestStatus.Accepted);

        public Task<RespondToInvitationResult> DeclineInvitationAsync(Guid donorId, Guid donationRequestId)
            => AnswerInvitationAsync(donorId, donationRequestId, DonationRequestStatus.Declined);

        private async Task<RespondToInvitationResult> AnswerInvitationAsync(
            Guid donorId,
            Guid donationRequestId,
            string newStatus)
        {
            var invitation = await _matchingRepo.GetDonationRequestAsync(donationRequestId);

            if (invitation == null) return RespondToInvitationResult.NotFound;

            // A donor can only answer their own invitation.
            if (invitation.DonorId != donorId) return RespondToInvitationResult.NotYours;

            if (invitation.Status != DonationRequestStatus.Invited)
            {
                return RespondToInvitationResult.AlreadyAnswered;
            }

            if (invitation.BloodRequest != null && invitation.BloodRequest.Status != "Pending")
            {
                return RespondToInvitationResult.RequestClosed;
            }

            invitation.Status = newStatus;
            await _matchingRepo.SaveChangesAsync();

            return RespondToInvitationResult.Success;
        }

        // ------------------------------------------------------------------
        // Hospital side
        // ------------------------------------------------------------------

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
               IsActive = x.Donor.User.IsActive,
               ResponseStatus = x.Status
           })
           .ToListAsync();

            // Donors who said yes belong at the top of the hospital's list.
            return responses
                .OrderByDescending(r => r.ResponseStatus == DonationRequestStatus.Accepted)
                .ThenBy(r => r.ResponseStatus == DonationRequestStatus.Declined)
                .ToList();
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
