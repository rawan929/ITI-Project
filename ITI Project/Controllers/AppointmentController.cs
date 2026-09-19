using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ITI.Web.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateAppointmentVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAppointmentVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _appointmentService.CreateAppointmentAsync(model);
            if (result)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "An Error occurred while saving the Appointment.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid bloodBankId = default)
        {
            var appointments = await _appointmentService.GetAppointmentsByBankAsync(bloodBankId);
            return View(appointments);
        }
    }
}