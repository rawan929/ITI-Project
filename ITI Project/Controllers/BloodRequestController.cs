using ITI.BLL.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using ITI.DAL.Context;
using Microsoft.EntityFrameworkCore;
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

            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(userIdString, out var userId))
                {
                    var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == userId);
                    if (donor != null)
                    {
                        respondedRequestIds = await _context.DonationRequests
                            .Where(dr => dr.DonorId == donor.Id)
                            .Select(dr => dr.BloodRequestId)
                            .ToHashSetAsync();
                    }
                }
            }

            ViewBag.RespondedRequestIds = respondedRequestIds;

            return View(requests);
        }
    }
}