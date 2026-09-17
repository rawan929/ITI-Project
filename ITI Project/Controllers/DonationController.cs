using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ITI.Web.Controllers
{
    public class DonationController : Controller
    {
        private readonly IDonationService _donationService;

        public DonationController(IDonationService donationService)
        {
            _donationService = donationService;
        }

       
        [HttpGet]
        public IActionResult Record()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Record(CreateDonationVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _donationService.RecordDonationAsync(model);
            if (result)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "An error occurred while recording the donation.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid bloodBankId)
        {
            var donations = await _donationService.GetDonationsByBankAsync(bloodBankId);
            return View(donations);
        }
    }
}