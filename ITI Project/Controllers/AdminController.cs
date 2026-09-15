using ITI.BLL.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITI.DAL.Context;

namespace ITI_Project.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly AppDbcontext _context;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // 1. تبويب Overview (الروت الرئيسي للـ Dashboard)
        public async Task<IActionResult> Index()
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            return View(stats);
        }

        // 2. تبويب Analytics (جديد: ليطابق تبويب Analytics بالفيجما)
        public async Task<IActionResult> Analytics()
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            return View(stats);
        }

        // 3. تبويب Users (معدل: ليطابق تبويب Users وشاشة Top Donors بالفيجما)
        [HttpGet]
        
        public async Task<IActionResult> Users()
        {
            var donors = await _adminService.GetAllDonorsAsync();
            return View(donors);
        }

        // 4. شاشة المستشفيات المعلقة
        public async Task<IActionResult> PendingHospitals()
        {
            var pendingHospitals = await _adminService.GetPendingHospitalsAsync();
            return View(pendingHospitals);
        }

        // 5. أكشن اعتماد مستشفى
        [HttpPost]
        public async Task<IActionResult> ApproveHospital(Guid id)
        {
            await _adminService.ApproveHospitalAsync(id);
            return RedirectToAction(nameof(PendingHospitals));
        }

        // 6. أكشن اعتماد بنك دم (جديد: لتغطية اعتماد بنوك الدم)
        [HttpPost]
        public async Task<IActionResult> ApproveBloodBank(Guid id)
        {
            await _adminService.ApproveBloodBankAsync(id);
            return RedirectToAction(nameof(PendingHospitals));
        }

        
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(Guid id)
        {
            await _adminService.ToggleUserStatusAsync(id);
            return RedirectToAction(nameof(Users));
        }
    }
}

