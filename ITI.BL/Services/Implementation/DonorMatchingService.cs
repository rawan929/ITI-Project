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
    public class DonorMatchingService : IDonorMatchingService
    {
        private readonly IDonorMatchingRepo _matchingRepo;
        private readonly IBloodBankRepo _bloodBankRepo;

        /// <summary>Same interval DonorService and BloodBankService use, so all three agree.</summary>
        private const int DonationIntervalDays = 90;

        public DonorMatchingService(IDonorMatchingRepo matchingRepo, IBloodBankRepo bloodBankRepo)
        {
            _matchingRepo = matchingRepo;
            _bloodBankRepo = bloodBankRepo;
        }

        public async Task<MatchingResultVM?> FindMatchingDonorsAsync(Guid bloodRequestId)
        {
            // 1. The request itself.
            var request = await _matchingRepo.GetBloodRequestAsync(bloodRequestId);
            if (request?.BloodType == null) return null;

            var hospitalCity = request.Hospital?.City ?? string.Empty;

            // 2. Blood type — which donor types can supply what was asked for.
            var compatibleTypeNames = BloodCompatibility.GetCompatibleDonorTypes(request.BloodType.Name);
            var compatibleTypeIds = await _matchingRepo.GetBloodTypeIdsByNamesAsync(compatibleTypeNames);

            var candidates = await _matchingRepo.GetCandidateDonorsAsync(compatibleTypeIds);

            // Donors who already have a row against this request.
            var alreadyContacted = (await _matchingRepo.GetExistingDonorIdsForRequestAsync(bloodRequestId))
                .ToHashSet();

            var today = DateTime.UtcNow.Date;

            // The blood bank donors will be sent to: nearest approved bank to the
            // hospital's city, or any approved bank if none exist there yet.
            var suggestedBanks = await _bloodBankRepo.GetApprovedBloodBanksAsync(hospitalCity);
            var suggestedBank = suggestedBanks.FirstOrDefault();
            if (suggestedBank == null)
            {
                var anyApproved = await _bloodBankRepo.GetApprovedBloodBanksAsync(null);
                suggestedBank = anyApproved.FirstOrDefault();
            }

            var donors = candidates
                // 3. Eligibility — 90 days since the last donation.
                .Where(d => IsEligible(d, today))
                // 4. Availability — there is no availability field on Donor yet, so
                //    the active-account check in the repo is all we can apply here.
                .Select(d => new MatchedDonorVM
                {
                    DonorId = d.Id,
                    FullName = d.User?.FullName ?? string.Empty,
                    BloodTypeName = d.BloodType?.Name ?? string.Empty,
                    City = d.User?.City ?? string.Empty,
                    PhoneNumber = d.User?.PhoneNumber ?? string.Empty,
                    TotalDonations = d.Donations?.Count ?? 0,
                    LastDonationDate = ResolveLastDonation(d),

                    // 5. Distance — the database stores a city, not coordinates, so
                    //    same-city is the closest thing to proximity we can compute.
                    //    Add Latitude/Longitude to Donor and Hospital to do this properly.
                    IsSameCity = !string.IsNullOrWhiteSpace(hospitalCity)
                                 && string.Equals(d.User?.City, hospitalCity, StringComparison.OrdinalIgnoreCase),

                    AlreadyContacted = alreadyContacted.Contains(d.Id),

                    SuggestedBloodBankId = suggestedBank?.Id,
                    SuggestedBloodBankName = suggestedBank?.Name ?? string.Empty
                })
                // 6. Sort — nearest first, then the exact blood type, then whoever
                //    has donated least recently so the load spreads out.
                .OrderByDescending(d => d.IsSameCity)
                .ThenByDescending(d => d.BloodTypeName == request.BloodType.Name)
                .ThenBy(d => d.LastDonationDate ?? DateTime.MinValue)
                .ToList();

            // 7. Result.
            return new MatchingResultVM
            {
                BloodRequestId = request.Id,
                HospitalName = request.Hospital?.Name ?? string.Empty,
                HospitalCity = hospitalCity,
                RequestedBloodType = request.BloodType.Name,
                UnitsRequired = request.UnitsRequired,
                Urgency = request.Urgency,
                CompatibleDonorTypes = compatibleTypeNames,
                Donors = donors
            };
        }

        private static DateTime? ResolveLastDonation(Donor donor)
        {
            var latest = donor.Donations?
                .OrderByDescending(x => x.DonationDate)
                .FirstOrDefault();

            return latest?.DonationDate ?? donor.LastDonationDate;
        }

        private static bool IsEligible(Donor donor, DateTime asOf)
        {
            var last = ResolveLastDonation(donor);
            if (last == null) return true;

            return (asOf.Date - last.Value.Date).TotalDays >= DonationIntervalDays;
        }
    }
}
