using ITI.BLL.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using ITI.DAL.Context;
using Microsoft.EntityFrameworkCore;
using ITI.BLL.ViewModel;
using System.Security.Claims;

namespace ITI_Project.Controllers
{
    public class BloodRequestController : Controller
    {
        private readonly IBloodRequestService _bloodRequestService;
        private readonly AppDbcontext _context;

        public BloodRequestController(IBloodRequestService bloodRequestService, AppDbcontext context)
        {
            _bloodRequestService = bloodRequestService;
            _context = context;
        }

        public async Task<IActionResult> Index(string city, int? bloodTypeId)
        {
            var bloodTypes = await _context.BloodTypes
                .OrderBy(x => x.Id)
                .ToListAsync();

            ViewBag.BloodTypes = bloodTypes;

            var requests = await _bloodRequestService
                .GetActiveRequestsAsync(city, bloodTypeId);

            
            var respondedRequestIds = new HashSet<Guid>();
            var declinedRequestIds = new HashSet<Guid>();

            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(userIdString, out var userId))
                {
                    var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == userId);
                    if (donor != null)
                    {
                        // The matching service quietly creates an "Invited" row for every
                        // compatible donor. That is only an invitation, NOT a response, so it
                        // must not show up as "Already Responded" - only rows the donor
                        // actually answered (Accepted / Scheduled / Declined) count.
                        var answered = await _context.DonationRequests
                            .Where(dr => dr.DonorId == donor.Id
                                         && dr.Status != DonationRequestStatus.Invited)
                            .Select(dr => new { dr.BloodRequestId, dr.Status })
                            .ToListAsync();

                        respondedRequestIds = answered
                            .Where(a => a.Status != DonationRequestStatus.Declined)
                            .Select(a => a.BloodRequestId)
                            .ToHashSet();

                        declinedRequestIds = answered
                            .Where(a => a.Status == DonationRequestStatus.Declined)
                            .Select(a => a.BloodRequestId)
                            .ToHashSet();
                    }
                }
            }

            ViewBag.RespondedRequestIds = respondedRequestIds;
            ViewBag.DeclinedRequestIds = declinedRequestIds;

            return View(requests);
        }
    }
}