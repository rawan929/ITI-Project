using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel.Donor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ITI.PL.Controllers
{
    [Authorize]
    public class DonorController : Controller
    {
        private readonly IDonorService _donorService;

        public DonorController(IDonorService donorService)
        {
            _donorService = donorService;
        }

        private Guid? GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdString, out var userId) ? userId : null;
        }

        // ===== Dashboard =====
        public async Task<IActionResult> Dashboard()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var dashboardData = await _donorService.GetDashboardData(userId.Value);
            if (dashboardData == null) return RedirectToAction("AccessDenied", "Account");

            var headerData = await _donorService.GetHeaderData(userId.Value, "Dashboard");
            ViewBag.Header = headerData;

            return View(dashboardData);
        }

        // ===== Profile =====
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var profileView = await _donorService.GetDonorProfileView(userId.Value);
            if (profileView == null) return RedirectToAction("AccessDenied", "Account");

            var headerData = await _donorService.GetHeaderData(userId.Value, "Profile");
            ViewBag.Header = headerData;

            return View(profileView);
        }

        // ===== Edit Profile =====
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var profile = await _donorService.GetDonorProfile(userId.Value);
            if (profile == null) return RedirectToAction("AccessDenied", "Account");

            var headerData = await _donorService.GetHeaderData(userId.Value, "Profile");
            ViewBag.Header = headerData;

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(DonorProfileViewModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                var profile = await _donorService.GetDonorProfile(userId.Value);
                model.AvailableBloodTypes = profile?.AvailableBloodTypes ?? new();
                ViewBag.Header = await _donorService.GetHeaderData(userId.Value, "Profile");
                return View(model);
            }

            var result = await _donorService.UpdateDonorProfile(userId.Value, model);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error);

                var profile = await _donorService.GetDonorProfile(userId.Value);
                model.AvailableBloodTypes = profile?.AvailableBloodTypes ?? new();
                ViewBag.Header = await _donorService.GetHeaderData(userId.Value, "Profile");
                return View(model);
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // ===== Change Password =====
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