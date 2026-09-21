using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
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
        private readonly IAppointmentService _appointmentService;
        private readonly AppDbcontext _context;

        public DonorController(
            IDonorService donorService,
            IBloodRequestService bloodRequestService,
            IDonationRequestService donationRequestService,
            IAppointmentService appointmentService,
            AppDbcontext context)
        {
            _donorService = donorService;
            _bloodRequestService = bloodRequestService;
            _donationRequestService = donationRequestService;
            _appointmentService = appointmentService;
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

        [HttpGet]
        public async Task<IActionResult> Appointments(bool sameCityOnly = true)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var model = await _appointmentService.GetBookingPageAsync(userId.Value, sameCityOnly);
            if (model == null) return RedirectToAction("AccessDenied", "Account");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(BookAppointmentVM model)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please choose a blood bank and a valid date.";
                return RedirectToAction(nameof(Appointments));
            }

            var result = await _appointmentService.BookAsync(userId.Value, model);

            switch (result)
            {
                case BookAppointmentResult.Success:
                    TempData["SuccessMessage"] = "Appointment booked. The blood bank can now see it.";
                    break;

                case BookAppointmentResult.AlreadyBooked:
                    TempData["ErrorMessage"] = "You already have an open appointment at that blood bank.";
                    break;

                case BookAppointmentResult.DateInThePast:
                    TempData["ErrorMessage"] = "Please choose a date and time in the future.";
                    break;

                case BookAppointmentResult.BloodBankNotFound:
                    TempData["ErrorMessage"] = "That blood bank isn't available for booking.";
                    break;

                default:
                    TempData["ErrorMessage"] = "We couldn't find your donor profile.";
                    break;
            }

            return RedirectToAction(nameof(Appointments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var cancelled = await _appointmentService.CancelAsync(userId.Value, id);

            TempData[cancelled ? "SuccessMessage" : "ErrorMessage"] = cancelled
                ? "Appointment cancelled."
                : "We couldn't cancel that appointment.";

            return RedirectToAction(nameof(Appointments));
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