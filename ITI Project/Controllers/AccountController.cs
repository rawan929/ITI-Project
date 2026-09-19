using Microsoft.AspNetCore.Mvc;
using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel.Account;
using System.Threading.Tasks;

namespace ITI.PL.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _authService.Login(model);

            if (result.Succeeded)
                return RedirectByUserType(result.UserType);

            ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault() ?? "Login failed");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            AuthResult result = model.UserType switch
            {
                "Donor" => await _authService.RegisterDonor(model),
                "Hospital" => await _authService.RegisterHospital(model),
                _ => new AuthResult { Succeeded = false, Errors = new[] { "Invalid account type or Blood Bank registration not ready yet" } }
            };

            if (result.Succeeded)
                return RedirectByUserType(result.UserType);

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _authService.Logout();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();

        private IActionResult RedirectByUserType(string? userType)
        {
            return userType switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Donor" => RedirectToAction("Dashboard", "Donor"),
                _ => RedirectToAction("Index", "Home")
            };
        }
    }
}
