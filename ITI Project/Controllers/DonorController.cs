using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel.Donor;
using ITI.DAL.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ITI.PL.Controllers
{
    [Authorize]
    public class DonorController : Controller
    {
        private readonly IDonorService _donorService;
        private readonly IBloodRequestService _bloodRequestService;
        private readonly IDonationRequestService _donationRequestService;
        private readonly AppDbcontext _context;

        public DonorController(
            IDonorService donorService,
            IBloodRequestService bloodRequestService,
            IDonationRequestService donationRequestService,
            AppDbcontext context)
        {
            _donorService = donorService;
            _bloodRequestService = bloodRequestService;
            _donationRequestService = donationRequestService;
            _context = context;
        }

        private Guid? GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdString, out var userId) ? userId : null;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var dashboardData = await _donorService.GetDashboardData(userId.Value);
            if (dashboardData == null) return RedirectToAction("AccessDenied", "Account");

            var activeRequests = await _bloodRequestService.GetActiveRequestsAsync(null, null);
            dashboardData.ActiveRequestsCount = activeRequests.Count();

            return View(dashboardData);
        }

        public async Task<IActionResult> History()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == userId.Value);
            if (donor == null) return RedirectToAction("AccessDenied", "Account");

            var history = await _donationRequestService.GetDonorResponseHistoryAsync(donor.Id);

            return View(history);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var profile = await _donorService.GetDonorProfile(userId.Value);
            if (profile == null) return RedirectToAction("AccessDenied", "Account");

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(DonorProfileViewModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                var profile = await _donorService.GetDonorProfile(userId.Value);
                model.AvailableBloodTypes = profile?.AvailableBloodTypes ?? new();
                return View(model);
            }

            var result = await _donorService.UpdateDonorProfile(userId.Value, model);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error);

                var profile = await _donorService.GetDonorProfile(userId.Value);
                model.AvailableBloodTypes = profile?.AvailableBloodTypes ?? new();
                return View(model);
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            var result = await _donorService.ChangePassword(userId.Value, model);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error);
                return View(model);
            }

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction("Profile");
        }
    }
}