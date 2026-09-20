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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Respond(Guid requestId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (donor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _donationRequestService.RespondToRequestAsync(donor.Id, requestId);

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
    }
}