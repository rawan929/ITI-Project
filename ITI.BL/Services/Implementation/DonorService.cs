using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel.Donor;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.BLL.Services.Implementation
{
    public class DonorService : IDonorService
    {
        private readonly IDonorRepo _donorRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public DonorService(IDonorRepo donorRepo, UserManager<ApplicationUser> userManager)
        {
            _donorRepo = donorRepo;
            _userManager = userManager;
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
                AvailableBloodTypes = bloodTypes.Select(b => new BloodTypeOption
                {
                    Id = b.Id,
                    Name = b.Name
                }).ToList()
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
    }
}