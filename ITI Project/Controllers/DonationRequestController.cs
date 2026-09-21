using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ITI_Project.Controllers
{
    [Authorize(Roles = "Donor")]
    public class DonationRequestController : Controller
    {
        private readonly IDonationRequestService _donationRequestService;
        private readonly AppDbcontext _context;

        public DonationRequestController(IDonationRequestService donationRequestService, AppDbcontext context)
        {
            _donationRequestService = donationRequestService;
            _context = context;
        }

        private async Task<Guid?> GetCurrentDonorIdAsync()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId)) return null;

            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == userId);
            return donor?.Id;
        }

        /// <summary>The donor's inbox: requests the system matched them to.</summary>
        public async Task<IActionResult> Index()
        {
            var donorId = await GetCurrentDonorIdAsync();
            if (donorId == null) return RedirectToAction("Login", "Account");

            var inbox = await _donationRequestService.GetInboxAsync(donorId.Value);
            return View(inbox);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(Guid id)
        {
            var donorId = await GetCurrentDonorIdAsync();
            if (donorId == null) return RedirectToAction("Login", "Account");

            var result = await _donationRequestService.AcceptInvitationAsync(donorId.Value, id);
            SetInvitationMessage(result, "Thank you! The hospital can now see that you accepted.");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decline(Guid id)
        {
            var donorId = await GetCurrentDonorIdAsync();
            if (donorId == null) return RedirectToAction("Login", "Account");

            var result = await _donationRequestService.DeclineInvitationAsync(donorId.Value, id);
            SetInvitationMessage(result, "Request declined. You won't be asked about this one again.");

            return RedirectToAction(nameof(Index));
        }

        /// <summary>Volunteering for a request found on the public request list.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Respond(Guid requestId)
        {
            var donorId = await GetCurrentDonorIdAsync();
            if (donorId == null) return RedirectToAction("Login", "Account");

            var result = await _donationRequestService.RespondToRequestAsync(donorId.Value, requestId);

            TempData["ResponseMessage"] = result switch
            {
                DonationResponseResult.Success => "Thank you! Your response has been recorded.",
                DonationResponseResult.AlreadyResponded => "You have already responded to this request.",
                DonationResponseResult.IncompatibleBloodType => "Your blood type is not compatible with this request.",
                _ => "Something went wrong. Please try again."
            };

            TempData["ResponseSuccess"] = (result == DonationResponseResult.Success).ToString();

            return RedirectToAction("Index", "BloodRequest");
        }

        private void SetInvitationMessage(RespondToInvitationResult result, string successMessage)
        {
            if (result == RespondToInvitationResult.Success)
            {
                TempData["SuccessMessage"] = successMessage;
                return;
            }

            TempData["ErrorMessage"] = result switch
            {
                RespondToInvitationResult.AlreadyAnswered => "You have already answered this request.",
                RespondToInvitationResult.RequestClosed => "That request has already been closed.",
                RespondToInvitationResult.NotYours => "That request isn't addressed to you.",
                _ => "We couldn't find that request."
            };
        }
    }
}
