using ITI.BLL.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using ITI.DAL.Context;
using Microsoft.EntityFrameworkCore;

namespace ITI_Project.Controllers
{
    public class BloodRequestController : Controller
    {
        private readonly IBloodRequestService _bloodRequestService;
        private readonly AppDbcontext _context;
        public BloodRequestController( IBloodRequestService bloodRequestService, AppDbcontext context)
        {
            _bloodRequestService = bloodRequestService;
            _context = context;
        } 
        public async Task<IActionResult> Index( string city, int? bloodTypeId)
        {
            var bloodTypes = await _context.BloodTypes
                .OrderBy(x => x.Id)
                .ToListAsync();

            ViewBag.BloodTypes = bloodTypes;

            var requests = await _bloodRequestService
                .GetActiveRequestsAsync(city, bloodTypeId);

            return View(requests);
        }


    }
}
