using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ITI.Web.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IBloodBankService _bloodBankService;

        public AppointmentController(IAppointmentService appointmentService, IBloodBankService bloodBankService)
        {
            _appointmentService = appointmentService;
            _bloodBankService = bloodBankService;
        }

        // ============ حجز موعد (المتبرع) ============
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadBanksAsync();
            return View(new BookAppointmentVM());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookAppointmentVM model)
        {
            if (!ModelState.IsValid)
            {
                await LoadBanksAsync();
                return View(model);
            }

            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Challenge();

            var (success, error) = await _appointmentService.BookAppointmentAsync(
                userId, model.BloodBankId!.Value, model.AppointmentDate!.Value);

            if (success)
            {
                TempData["Success"] = "Your appointment has been booked.";
                return RedirectToAction(nameof(Create));   
            }

            ModelState.AddModelError(string.Empty, error ?? "An error occurred while saving the appointment.");
            await LoadBanksAsync();
            return View(model);
        }

        // ============ مواعيد البنك ============
        [HttpGet]
        public async Task<IActionResult> Index(Guid bloodBankId = default, DateTime? date = null)
        {
            var day = (date ?? DateTime.Today).Date;
            ViewBag.BloodBankId = bloodBankId;
            ViewBag.Date = day;

            var appointments = await _appointmentService.GetAppointmentsByBankAsync(bloodBankId, day);
            return View(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(Guid id, Guid bloodBankId, string status, DateTime? date)
        {
            var (success, error) = await _appointmentService.ChangeStatusAsync(id, bloodBankId, status);
            if (!success) TempData["Error"] = error;

            return RedirectToAction(nameof(Index), new { bloodBankId, date });
        }

        private async Task LoadBanksAsync()
        {
            var banks = await _bloodBankService.GetBloodBanksAsync();
            ViewBag.Banks = new SelectList(banks, "Id", "Name");
        }
    }
}