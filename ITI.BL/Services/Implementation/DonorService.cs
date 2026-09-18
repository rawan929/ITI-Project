using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel.Donor;
using ITI.DAL.Repo.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.BLL.Services.Implementation
{
    public class DonorService : IDonorService
    {
        private readonly IDonorRepo _donorRepo;

        public DonorService(IDonorRepo donorRepo)
        {
            _donorRepo = donorRepo;
        }

        public async Task<DonorDashboardViewModel?> GetDashboardData(Guid userId)
        {
            var donor = await _donorRepo.GetByUserIdWithDetailsAsync(userId);

            if (donor == null) return null;

            var lastDonation = donor.Donations
                .OrderByDescending(d => d.DonationDate)
                .FirstOrDefault();

            DateTime? lastDonationDate = lastDonation?.DonationDate ?? donor.LastDonationDate;

            DateTime? nextEligible = null;
            int? daysLeft = null;
            bool isEligible = true;

            if (lastDonationDate.HasValue)
            {
                nextEligible = lastDonationDate.Value.AddDays(90);
                var remaining = (nextEligible.Value - DateTime.UtcNow).Days;
                isEligible = remaining <= 0;
                daysLeft = remaining > 0 ? remaining : null;
            }

            return new DonorDashboardViewModel
            {
                FullName = donor.User.FullName,
                MemberSince = donor.User.CreatedAt.ToString("MMMM yyyy"),
                IsEligible = isEligible,
                BloodTypeName = donor.BloodType?.Name ?? "Not set",
                TotalDonations = donor.Donations.Count,
                LastDonationDate = lastDonationDate,
                NextEligibleDate = nextEligible,
                DaysUntilEligible = daysLeft
            };
        }
    }
}