using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public HospitalController(IDonationRequestService donationRequestService, AppDbcontext context, IBloodRequestService bloodRequestService)
        {
            _donationRequestService = donationRequestService;
            _context = context;
            _bloodRequestService = bloodRequestService;
        }

        public async Task<IActionResult> Dashboard()
        {


            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

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
        public async Task<IActionResult> CreateRequest( CreateBloodRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var hospital = await _context.Hospitals
                .FirstOrDefaultAsync(h => h.UserId == userId);

            if (hospital == null)
            {
                return NotFound();
            }

            await _bloodRequestService.CreateRequestAsync(model, hospital.Id);


            return RedirectToAction(nameof(Dashboard));
        }

    }
}
