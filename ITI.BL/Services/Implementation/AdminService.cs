using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Context;
using ITI.DAL.Models;
using ITI.DAL.Repo.Implementation;
using ITI.DAL.Repo.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ITI.BLL.Services.Implementation
{
    public class AdminService : IAdminService
    {
        private readonly IDonorRepo _donorRepo;
        private readonly IHospitalRepo _hospitalRepo;
        private readonly IBloodBankRepo _bloodBankRepo;
        private readonly AppDbcontext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private IDonorRepo? donorRepo;

        public AdminService(
     IDonorRepo donorRepo,
     IHospitalRepo hospitalRepo,
     IBloodBankRepo bloodBankRepo,
     AppDbcontext context,
     UserManager<ApplicationUser> userManager)
        {
            _donorRepo = donorRepo;
            _hospitalRepo = hospitalRepo;
            _bloodBankRepo = bloodBankRepo;
            _context = context;
            _userManager = userManager;
        }

        public async Task<AdminDashboardVm> GetDashboardStatsAsync()
        {
            return new AdminDashboardVm
            {
                TotalDonors = await _context.Donors.CountAsync(),
                ApprovedHospitals = await _hospitalRepo.GetApprovedCountAsync(),
                PendingHospitals = await _hospitalRepo.GetPendingCountAsync(),
                ApprovedBloodBanks = await _bloodBankRepo.GetApprovedCountAsync(),
                ActiveBloodRequests = await _context.BloodRequests.CountAsync(r => r.Status == "Pending")
            };
        }
        public async Task<List<DonorVM>> GetAllDonorsAsync()
        {
            var donors = await _donorRepo.GetAllDonorsWithDetailsAsync();

            return donors.Select(d => new DonorVM
            {
                Id = d.User?.Id ?? Guid.Empty,
                FullName = d.User?.FullName ?? "",
                BloodType = d.BloodType?.Name ?? "",
                City = d.User?.City ?? "",
                TotalDonations = d.Donations?.Count ?? 0,
                IsActive = d.User?.IsActive ?? false
            })
            .OrderByDescending(d => d.TotalDonations)   
            .ToList();
        }

        public async Task<List<PendingApprovalsVM>> GetPendingHospitalsAsync()
        {
            var pendingHospitals = await _hospitalRepo.GetPendingHospitalsAsync();

            return pendingHospitals.Select(h => new PendingApprovalsVM
            {
                Id = h.Id,
                Name = h.Name,
                City = h.City,
                Address = h.Address,
                Type = "Hospital",
                Email = h.User?.Email ?? "",
                PhoneNumber = h.User?.PhoneNumber ?? "",
                IsApproved = h.IsApproved
            }).ToList();
        }

        public async Task<bool> ApproveHospitalAsync(Guid hospitalId)
        {
            var hospital = await _hospitalRepo.GetByIdAsync(hospitalId);
            if (hospital == null) return false;

            hospital.IsApproved = true;
            await _hospitalRepo.UpdateHospitalAsync(hospital);
            return true;
        }

        public async Task<bool> ApproveBloodBankAsync(Guid bloodBankId)
        {
            var bloodBank = await _bloodBankRepo.GetByIdAsync(bloodBankId);
            if (bloodBank == null) return false;

            bloodBank.IsApproved = true;
            await _bloodBankRepo.UpdateBloodBankAsync(bloodBank);
            return true;
        }

        public async Task<bool> ToggleUserStatusAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }
    }
}

