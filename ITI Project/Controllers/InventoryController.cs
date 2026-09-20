using ITI.BLL.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ITI.Web.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid bloodBankId = default)
        {
            var model = await _inventoryService.GetInventoryVMAsync(bloodBankId);
            if (model.BloodBankId == Guid.Empty) return NotFound();
            return View(model);
        }
    }
}