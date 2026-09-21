using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Repo.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ITI_Project.Controllers
{
    [Authorize(Roles = "BloodBank")]
    public class BloodBankController : Controller
    {
        private readonly IBloodBankService _bloodBankService;
        private readonly IBloodBankRepo _bloodBankRepo;

        public BloodBankController(
            IBloodBankService bloodBankService,
            IBloodBankRepo bloodBankRepo)
        {
            _bloodBankService = bloodBankService;
            _bloodBankRepo = bloodBankRepo;
        }

        private Guid? GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdString, out var userId) ? userId : null;
        }

        // Runs before every action here, so all views know whether the admin
        // has approved this blood bank yet (same idea as HospitalController).
        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var userId = GetCurrentUserId();

            if (userId != null)
            {
                var bank = await _bloodBankRepo.GetByUserIdAsync(userId.Value);
                ViewBag.IsBloodBankApproved = bank?.IsApproved ?? false;
                ViewBag.BloodBankName = bank?.Name ?? "Blood Bank";
            }
            else
            {
                ViewBag.IsBloodBankApproved = false;
                ViewBag.BloodBankName = "Blood Bank";
            }

            await next();
        }

        // ------------------------------------------------------------------
        // Overview
        // ------------------------------------------------------------------

        public async Task<IActionResult> Dashboard()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var dashboard = await _bloodBankService.GetDashboardAsync(userId.Value);
            if (dashboard == null) return NotFound();

            return View(dashboard);
        }

        // ------------------------------------------------------------------
        // Inventory
        // ------------------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Inventory()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var model = await _bloodBankService.GetInventoryPageAsync(userId.Value);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustInventory(BloodBankAdjustInventoryVM model)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please choose a blood type and a valid number of units.";
                return RedirectToAction(nameof(Inventory));
            }

            var result = await _bloodBankService.AdjustInventoryAsync(userId.Value, model);

            switch (result)
            {
                case AdjustInventoryResult.Success:
                    TempData["SuccessMessage"] = "Inventory updated successfully.";
                    break;

                case AdjustInventoryResult.BloodBankNotApproved:
                    TempData["ErrorMessage"] =
                        "Your blood bank is still pending admin approval, so you can't change stock yet.";
                    break;

                case AdjustInventoryResult.InsufficientStock:
                    TempData["ErrorMessage"] =
                        "You don't have that many units in stock for this blood type.";
                    break;

                case AdjustInventoryResult.InvalidBloodType:
                    TempData["ErrorMessage"] = "That blood type doesn't exist.";
                    break;

                default:
                    TempData["ErrorMessage"] = "We couldn't find your blood bank account.";
                    break;
            }

            return RedirectToAction(nameof(Inventory));
        }

        // ------------------------------------------------------------------
        // Donations
        // ------------------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> RecordDonation(bool sameCityOnly = false)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var model = await _bloodBankService.GetRecordDonationFormAsync(userId.Value, sameCityOnly);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordDonation(BloodBankRecordDonationVM model)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                return View(await RefillDonorsAsync(userId.Value, model));
            }

            var result = await _bloodBankService.RecordDonationAsync(userId.Value, model);

            if (result == RecordDonationResult.Success)
            {
                TempData["SuccessMessage"] =
                    "Donation recorded. The units were added to your inventory.";
                return RedirectToAction(nameof(Donations));
            }

            ModelState.AddModelError(string.Empty, result switch
            {
                RecordDonationResult.BloodBankNotApproved =>
                    "Your blood bank is still pending admin approval, so you can't record donations yet.",
                RecordDonationResult.DonorNotFound =>
                    "We couldn't find that donor.",
                RecordDonationResult.DonorBloodTypeMissing =>
                    "This donor hasn't set a blood type yet. Ask them to complete their profile first.",
                RecordDonationResult.DonorNotEligible =>
                    "This donor isn't eligible yet — it has been less than 90 days since their last donation.",
                RecordDonationResult.InvalidDate =>
                    "The donation date can't be in the future.",
                _ => "We couldn't find your blood bank account."
            });

            return View(await RefillDonorsAsync(userId.Value, model));
        }

        public async Task<IActionResult> Donations()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var donations = await _bloodBankService.GetDonationHistoryAsync(userId.Value);
            return View(donations);
        }

        // ------------------------------------------------------------------
        // Appointments
        // ------------------------------------------------------------------

        public async Task<IActionResult> Appointments()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var appointments = await _bloodBankService.GetAppointmentsAsync(userId.Value);
            return View(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointmentStatus(Guid id, string status)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var updated = await _bloodBankService.UpdateAppointmentStatusAsync(userId.Value, id, status);

            TempData[updated ? "SuccessMessage" : "ErrorMessage"] = updated
                ? $"Appointment marked as {status}."
                : "We couldn't update that appointment.";

            return RedirectToAction(nameof(Appointments));
        }

        // ------------------------------------------------------------------
        // Hospital requests
        // ------------------------------------------------------------------

        public async Task<IActionResult> Requests(bool sameCityOnly = true)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            ViewBag.SameCityOnly = sameCityOnly;

            var requests = await _bloodBankService.GetIncomingRequestsAsync(userId.Value, sameCityOnly);
            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FulfillRequest(Guid id, bool sameCityOnly = true)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var result = await _bloodBankService.FulfillRequestAsync(userId.Value, id);

            switch (result)
            {
                case FulfillRequestResult.Success:
                    TempData["SuccessMessage"] =
                        "Request fulfilled. The units were deducted from your inventory.";
                    break;

                case FulfillRequestResult.InsufficientStock:
                    TempData["ErrorMessage"] =
                        "You don't have enough units of that blood type to cover this request.";
                    break;

                case FulfillRequestResult.RequestAlreadyClosed:
                    TempData["ErrorMessage"] = "That request has already been closed.";
                    break;

                case FulfillRequestResult.BloodBankNotApproved:
                    TempData["ErrorMessage"] =
                        "Your blood bank is still pending admin approval.";
                    break;

                default:
                    TempData["ErrorMessage"] = "We couldn't fulfil that request.";
                    break;
            }

            return RedirectToAction(nameof(Requests), new { sameCityOnly });
        }

        // ------------------------------------------------------------------
        // Profile
        // ------------------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var profile = await _bloodBankService.GetProfileAsync(userId.Value);
            if (profile == null) return NotFound();

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(BloodBankProfileVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var success = await _bloodBankService.UpdateProfileAsync(userId.Value, model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Could not update profile.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }

        // ------------------------------------------------------------------

        /// <summary>
        /// Re-populates the donor dropdown after a failed post, since the browser
        /// only sends back the selected id, not the whole list.
        /// </summary>
        private async Task<BloodBankRecordDonationVM> RefillDonorsAsync(
            Guid userId,
            BloodBankRecordDonationVM model)
        {
            var fresh = await _bloodBankService.GetRecordDonationFormAsync(userId, model.SameCityOnly);
            model.AvailableDonors = fresh?.AvailableDonors ?? new List<BloodBankDonorOptionVM>();
            model.BankCity = fresh?.BankCity ?? string.Empty;
            return model;
        }
    }
}
