using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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

            if (result == RespondToInvitationResult.Success)
            {
                // Straight to booking a slot instead of back to the inbox —
                // accepting isn't done until there's an actual appointment.
                return RedirectToAction(nameof(Schedule), new { id });
            }

            SetInvitationMessage(result, string.Empty);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Schedule(Guid id)
        {
            var donorId = await GetCurrentDonorIdAsync();
            if (donorId == null) return RedirectToAction("Login", "Account");

            var invitation = await _context.DonationRequests
                .Include(x => x.BloodBank)
                .Include(x => x.BloodRequest).ThenInclude(x => x.Hospital)
                .Include(x => x.BloodRequest).ThenInclude(x => x.BloodType)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (invitation == null || invitation.DonorId != donorId) return NotFound();

            if (invitation.Status != DonationRequestStatus.Accepted)
            {
                // Already scheduled, declined, or still just invited — nothing to book here.
                return RedirectToAction(nameof(Index));
            }

            var availableBanks = await _donationRequestService.GetAvailableBloodBanksAsync(invitation.Id);

            if (!availableBanks.Any())
            {
                TempData["ErrorMessage"] = "No blood bank is available to handle this donation right now. Please contact support.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new ScheduleAppointmentVM
            {
                DonationRequestId = invitation.Id,
                HospitalName = invitation.BloodRequest.Hospital.Name,
                BloodTypeName = invitation.BloodRequest.BloodType.Name,
                AvailableBloodBanks = availableBanks,
                // Preselect the previously suggested bank if it's still in the list, else the top (nearest) one.
                BloodBankId = availableBanks.Any(b => b.BloodBankId == invitation.BloodBankId)
                    ? invitation.BloodBankId
                    : availableBanks.First().BloodBankId
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(ScheduleAppointmentVM model)
        {
            var donorId = await GetCurrentDonorIdAsync();
            if (donorId == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                model.AvailableBloodBanks = await _donationRequestService.GetAvailableBloodBanksAsync(model.DonationRequestId);
                return View(model);
            }

            var result = await _donationRequestService
                .ScheduleAppointmentAsync(donorId.Value, model.DonationRequestId, model.BloodBankId!.Value, model.AppointmentDate);

            if (result == ScheduleAppointmentResult.Success)
            {
                TempData["SuccessMessage"] = "Appointment booked! The blood bank can now see it on their schedule.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result switch
            {
                ScheduleAppointmentResult.DateInThePast => "Please choose a future date and time.",
                ScheduleAppointmentResult.AlreadyScheduled => "You already booked an appointment for this request.",
                ScheduleAppointmentResult.NotAccepted => "You need to accept this request first.",
                ScheduleAppointmentResult.NoBloodBankAssigned => "That blood bank isn't available anymore. Please choose another one.",
                _ => "We couldn't book that appointment."
            });

            model.AvailableBloodBanks = await _donationRequestService.GetAvailableBloodBanksAsync(model.DonationRequestId);
            return View(model);
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
