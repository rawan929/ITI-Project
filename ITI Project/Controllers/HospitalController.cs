using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ITI_Project.Controllers
{
    [Authorize(Roles = "Hospital")]
    public class HospitalController : Controller
    {
        private readonly IDonationRequestService _donationRequestService;
        private readonly AppDbcontext _context;
        private readonly IBloodRequestService _bloodRequestService;
        private readonly IHospitalService _hospitalService;

        public HospitalController(
            IDonationRequestService donationRequestService,
            AppDbcontext context,
            IBloodRequestService bloodRequestService,
            IHospitalService hospitalService)
        {
            _donationRequestService = donationRequestService;
            _context = context;
            _bloodRequestService = bloodRequestService;
            _hospitalService = hospitalService;
        }

        private Guid? GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdString, out var userId) ? userId : null;
        }

        // بيشتغل تلقائيًا قبل أي Action في الكونترولر ده
        // بيحط في ViewBag.IsHospitalApproved حالة الاعتماد، عشان كل الـ Views تقدر تستخدمها
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userId = GetCurrentUserId();
            if (userId != null)
            {
                var hospital = await _context.Hospitals
                    .FirstOrDefaultAsync(h => h.UserId == userId);

                ViewBag.IsHospitalApproved = hospital?.IsApproved ?? false;
            }
            else
            {
                ViewBag.IsHospitalApproved = false;
            }

            await next();
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var hospital = await _context.Hospitals
                .FirstOrDefaultAsync(h => h.UserId == userId);

            if (hospital == null)
            {
                return NotFound();
            }

            var requests = await _context.BloodRequests
                .Include(r => r.BloodType)
                .Where(r => r.HospitalId == hospital.Id)
                .ToListAsync();

            return View(requests);
        }

        /// <summary>Shows who the system matched for a request, without inviting anyone new.</summary>
        public async Task<IActionResult> MatchedDonors(Guid id)
        {
            var matches = await _bloodRequestService.PreviewMatchesAsync(id);
            if (matches == null) return NotFound();

            return View(matches);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NotifyMatchedDonors(Guid id)
        {
            var notified = await _bloodRequestService.MatchAndNotifyAsync(id);

            TempData["SuccessMessage"] = notified > 0
                ? $"{notified} additional donor(s) were notified."
                : "No new donors to notify for this request.";

            return RedirectToAction(nameof(MatchedDonors), new { id });
        }

        public async Task<IActionResult> ViewResponses(Guid id)
        {
            var responses = await _donationRequestService
                .GetResponsesForRequestAsync(id);

            return View(responses);
        }

        [HttpGet]
        public IActionResult CreateRequest()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest(CreateBloodRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var hospital = await _context.Hospitals
                .FirstOrDefaultAsync(h => h.UserId == userId);

            if (hospital == null)
            {
                return NotFound();
            }

            var result = await _bloodRequestService.CreateRequestAsync(model, hospital.Id);

            switch (result.Result)
            {
                case CreateRequestResult.Success:
                    TempData["SuccessMessage"] = result.NotifiedDonors > 0
                        ? $"Blood request created. {result.NotifiedDonors} matching donor(s) were notified."
                        : "Blood request created, but no eligible matching donor was found yet.";
                    return RedirectToAction(nameof(Dashboard));

                case CreateRequestResult.HospitalNotApproved:
                    ModelState.AddModelError(string.Empty,
                        "Your hospital account is still pending approval from the admin. You cannot create requests yet.");
                    return View(model);

                case CreateRequestResult.HospitalNotFound:
                default:
                    ModelState.AddModelError(string.Empty,
                        "We couldn't find your hospital account. Please contact support.");
                    return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var profile = await _hospitalService.GetHospitalProfileAsync(userId.Value);
            if (profile == null) return NotFound();

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(HospitalProfileVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var success = await _hospitalService.UpdateHospitalProfileAsync(userId.Value, model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Could not update profile.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }
    }
}