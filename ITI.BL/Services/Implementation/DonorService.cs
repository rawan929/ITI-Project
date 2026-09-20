using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel.Donor;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITI.BLL.Services.Implementation
{
    public class DonorService : IDonorService
    {
        private readonly IDonorRepo _donorRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBloodRequestService _bloodRequestService;
        private readonly IDonationRequestService _donationRequestService;

        public DonorService(
            IDonorRepo donorRepo,
            UserManager<ApplicationUser> userManager,
            IBloodRequestService bloodRequestService,
            IDonationRequestService donationRequestService)
        {
            _donorRepo = donorRepo;
            _userManager = userManager;
            _bloodRequestService = bloodRequestService;
            _donationRequestService = donationRequestService;
        }

        private (bool isEligible, DateTime? nextEligible, int? daysLeft) CalculateEligibility(DateTime? lastDonationDate)
        {
            if (!lastDonationDate.HasValue)
                return (true, null, null);

            var nextEligible = lastDonationDate.Value.AddDays(90);
            var remaining = (nextEligible - DateTime.UtcNow).Days;
            var isEligible = remaining <= 0;
            var daysLeft = remaining > 0 ? remaining : (int?)null;

            return (isEligible, nextEligible, daysLeft);
        }

        public async Task<DonorDashboardViewModel?> GetDashboardData(Guid userId)
        {
            var donor = await _donorRepo.GetByUserIdWithDetailsAsync(userId);
            if (donor == null) return null;

            var lastDonation = donor.Donations.OrderByDescending(d => d.DonationDate).FirstOrDefault();
            DateTime? lastDonationDate = lastDonation?.DonationDate ?? donor.LastDonationDate;

            var (isEligible, nextEligible, daysLeft) = CalculateEligibility(lastDonationDate);

            
            var nearbyRequests = new List<NearbyRequestItemViewModel>();
            try
            {
                var requests = await _bloodRequestService.GetActiveRequestsAsync(donor.User.City, donor.BloodTypeId);
                nearbyRequests = requests
                    .Take(3)
                    .Select(r => new NearbyRequestItemViewModel
                    {
                        Id = r.Id,
                        HospitalName = r.Hospital?.Name ?? "Unknown Hospital",
                        BloodTypeName = r.BloodType?.Name ?? "N/A",
                        UnitsRequired = r.UnitsRequired,
                        Urgency = r.Urgency,
                        Status = r.Status
                    }).ToList();
            }
            catch
            {
                nearbyRequests = new List<NearbyRequestItemViewModel>();
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
                DaysUntilEligible = daysLeft,
                NearbyRequests = nearbyRequests
            };
        }

        public async Task<DonorHeaderViewModel?> GetHeaderData(Guid userId, string activeTab)
        {
            var donor = await _donorRepo.GetByUserIdWithDetailsAsync(userId);
            if (donor == null) return null;

            var lastDonation = donor.Donations.OrderByDescending(d => d.DonationDate).FirstOrDefault();
            DateTime? lastDonationDate = lastDonation?.DonationDate ?? donor.LastDonationDate;

            var (isEligible, _, _) = CalculateEligibility(lastDonationDate);

            return new DonorHeaderViewModel
            {
                FullName = donor.User.FullName,
                MemberSince = donor.User.CreatedAt.ToString("MMMM yyyy"),
                IsEligible = isEligible,
                ActiveTab = activeTab
            };
        }

        public async Task<DonorProfileViewModel?> GetDonorProfile(Guid userId)
        {
            var donor = await _donorRepo.GetByUserIdWithDetailsAsync(userId);
            if (donor == null) return null;

            var bloodTypes = await _donorRepo.GetAllBloodTypesAsync();

            return new DonorProfileViewModel
            {
                FullName = donor.User.FullName,
                Email = donor.User.Email ?? string.Empty,
                Phone = donor.User.PhoneNumber ?? string.Empty,
                City = donor.User.City,
                BloodTypeId = donor.BloodTypeId,
                DateOfBirth = donor.DateOfBirth,
                Gender = donor.Gender,
                Weight = donor.Weight,
                Height = donor.Height,
                KnownAllergies = donor.KnownAllergies,
                ChronicConditions = donor.ChronicConditions,
                AvailableBloodTypes = bloodTypes.Select(b => new BloodTypeOption
                {
                    Id = b.Id,
                    Name = b.Name
                }).ToList()
            };
        }

        public async Task<DonorProfileViewViewModel?> GetDonorProfileView(Guid userId)
        {
            var donor = await _donorRepo.GetByUserIdWithDetailsAsync(userId);
            if (donor == null) return null;

            var lastDonation = donor.Donations.OrderByDescending(d => d.DonationDate).FirstOrDefault();
            DateTime? lastDonationDate = lastDonation?.DonationDate ?? donor.LastDonationDate;

            var (isEligible, nextEligible, _) = CalculateEligibility(lastDonationDate);

            return new DonorProfileViewViewModel
            {
                FullName = donor.User.FullName,
                Email = donor.User.Email ?? string.Empty,
                Phone = donor.User.PhoneNumber ?? string.Empty,
                City = donor.User.City,
                BloodTypeName = donor.BloodType?.Name ?? "Not set",
                DateOfBirth = donor.DateOfBirth,
                LastDonationDate = lastDonationDate,
                EligibleFromDate = nextEligible,
                IsEligible = isEligible,
                Weight = donor.Weight,
                Height = donor.Height,
                KnownAllergies = donor.KnownAllergies,
                ChronicConditions = donor.ChronicConditions
            };
        }

        public async Task<AuthResult> UpdateDonorProfile(Guid userId, DonorProfileViewModel model)
        {
            var donor = await _donorRepo.GetByUserIdWithDetailsAsync(userId);
            if (donor == null)
            {
                return new AuthResult { Succeeded = false, Errors = new[] { "Donor not found" } };
            }

            donor.User.FullName = model.FullName;
            donor.User.PhoneNumber = model.Phone;
            donor.User.City = model.City;
            await _userManager.UpdateAsync(donor.User);

            donor.BloodTypeId = model.BloodTypeId;
            donor.DateOfBirth = model.DateOfBirth;
            donor.Gender = model.Gender;
            donor.Weight = model.Weight;
            donor.Height = model.Height;
            donor.KnownAllergies = model.KnownAllergies;
            donor.ChronicConditions = model.ChronicConditions;
            await _donorRepo.UpdateDonorAsync(donor);

            return new AuthResult { Succeeded = true };
        }

        public async Task<AuthResult> ChangePassword(Guid userId, ChangePasswordViewModel model)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new AuthResult { Succeeded = false, Errors = new[] { "User not found" } };
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return new AuthResult { Succeeded = false, Errors = errors };
            }

            return new AuthResult { Succeeded = true };
        }


        public async Task<List<NearbyRequestItemViewModel>> GetFindRequestsData(Guid userId, string? urgencyFilter)
        {
            var donor = await _donorRepo.GetByUserIdWithDetailsAsync(userId);
            if (donor == null) return new List<NearbyRequestItemViewModel>();

            var requests = await _bloodRequestService.GetActiveRequestsAsync(donor.User.City, donor.BloodTypeId);

            var list = requests.Select(r => new NearbyRequestItemViewModel
            {
                Id = r.Id,
                HospitalName = r.Hospital?.Name ?? "Unknown Hospital",
                BloodTypeName = r.BloodType?.Name ?? "N/A",
                UnitsRequired = r.UnitsRequired,
                Urgency = r.Urgency,
                Status = r.Status
            });

            if (!string.IsNullOrEmpty(urgencyFilter) && urgencyFilter != "All")
            {
                list = list.Where(r => r.Urgency.Equals(urgencyFilter, StringComparison.OrdinalIgnoreCase));
            }

            return list.ToList();
        }

        public async Task<AuthResult> RespondToRequest(Guid userId, Guid requestId)
        {
            var donor = await _donorRepo.GetByUserIdWithDetailsAsync(userId);
            if (donor == null)
            {
                return new AuthResult { Succeeded = false, Errors = new[] { "Donor not found" } };
            }

            var success = await _donationRequestService.RespondToRequestAsync(donor.Id, requestId);

            if (!success)
            {
                return new AuthResult { Succeeded = false, Errors = new[] { "Could not respond to this request. It may no longer be active." } };
            }

            return new AuthResult { Succeeded = true };
        }

        public async Task<string> GetDonorBloodTypeName(Guid userId)
        {
            var donor = await _donorRepo.GetByUserIdWithDetailsAsync(userId);
            return donor?.BloodType?.Name ?? "your blood type";
        }
    }
}