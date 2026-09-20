using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using Microsoft.AspNetCore.Mvc;

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
            return View(new CreateDonationVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Record(CreateDonationVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _donationService.RecordDonationAsync(model);
            if (result)
                return RedirectToAction(nameof(Index), new { bloodBankId = model.BloodBankId });   
            ModelState.AddModelError("", "An error occurred while recording the donation.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid bloodBankId = default)
        {
            ViewBag.BloodBankId = bloodBankId;
            var donations = await _donationService.GetDonationsByBankAsync(bloodBankId);
            return View(donations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkProcessed(Guid id, Guid bloodBankId)
        {
            var (success, error) = await _donationService.MarkProcessedAsync(id, bloodBankId);

            if (success) TempData["Success"] = "Donation processed and added to the inventory.";
            else TempData["Error"] = error;

            return RedirectToAction(nameof(Index), new { bloodBankId });
        }
    }
}