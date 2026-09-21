using ITI.DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITI.DAL.Repo.Interface
{
    /// <summary>
    /// Data access for turning a hospital blood request into a list of donors,
    /// and for the invitations that come out of it.
    /// </summary>
    public interface IDonorMatchingRepo
    {
        Task<BloodRequest?> GetBloodRequestAsync(Guid bloodRequestId);

        /// <summary>Active donors whose blood type is one of the supplied ids.</summary>
        Task<List<Donor>> GetCandidateDonorsAsync(List<int> bloodTypeIds);

        Task<List<int>> GetBloodTypeIdsByNamesAsync(List<string> names);

        /// <summary>Donors who already have a row against this request, so they aren't invited twice.</summary>
        Task<List<Guid>> GetExistingDonorIdsForRequestAsync(Guid bloodRequestId);

        Task AddDonationRequestsAsync(List<DonationRequest> donationRequests);

        Task<List<DonationRequest>> GetDonationRequestsForDonorAsync(Guid donorId);

        Task<DonationRequest?> GetDonationRequestAsync(Guid donationRequestId);

        Task SaveChangesAsync();
    }
}
