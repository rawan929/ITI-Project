using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.BLL.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly IDonorMatchingRepo _matchingRepo;

        /// <summary>
        /// Cap on how many donors one request invites at once. Without a cap an
        /// O- request would notify most of the donor base for a two-unit need.
        /// </summary>
        private const int MaxDonorsPerRequest = 25;

        public NotificationService(IDonorMatchingRepo matchingRepo)
        {
            _matchingRepo = matchingRepo;
        }

        public async Task<int> NotifyMatchingDonorsAsync(Guid bloodRequestId, List<MatchedDonorVM> donors)
        {
            if (donors == null || donors.Count == 0) return 0;

            // Re-read from the database rather than trusting the caller's flag, so two
            // requests running at once can't create duplicate invitations.
            var alreadyContacted = (await _matchingRepo.GetExistingDonorIdsForRequestAsync(bloodRequestId))
                .ToHashSet();

            var toInvite = donors
                .Where(d => !alreadyContacted.Contains(d.DonorId))
                .Take(MaxDonorsPerRequest)
                .Select(d => new DonationRequest
                {
                    Id = Guid.NewGuid(),
                    BloodRequestId = bloodRequestId,
                    DonorId = d.DonorId,
                    Status = DonationRequestStatus.Invited,
                    BloodBankId = d.SuggestedBloodBankId
                })
                .ToList();

            if (toInvite.Count == 0) return 0;

            await _matchingRepo.AddDonationRequestsAsync(toInvite);
            await _matchingRepo.SaveChangesAsync();

            return toInvite.Count;
        }
    }
}
