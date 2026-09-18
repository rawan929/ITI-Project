using ITI.BLL.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ITI.PL.Controllers
{
    [Authorize] 
    public class DonorController : Controller
    {
        private readonly IDonorService _donorService;

        public DonorController(IDonorService donorService)
        {
            _donorService = donorService;
        }

        public async Task<IActionResult> Dashboard()
        {
            
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var dashboardData = await _donorService.GetDashboardData(userId);

            if (dashboardData == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View(dashboardData);
        }
    }
}