using ITI.BLL.ViewModel.Account;
using ITI.BLL.Services.Interface;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITI.BLL.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IDonorRepo _donorRepo;
        private readonly IHospitalRepo _hospitalRepo;
        private readonly IBloodBankRepo _bloodBankRepo;

        public AuthService(UserManager<ApplicationUser> userManager,
                            SignInManager<ApplicationUser> signInManager,
                            IDonorRepo donorRepo,
                            IHospitalRepo hospitalRepo,
                            IBloodBankRepo bloodBankRepo)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _donorRepo = donorRepo;
            _hospitalRepo = hospitalRepo;
            _bloodBankRepo = bloodBankRepo;
        }

        public async Task<AuthResult> RegisterDonor(RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.Phone,
                City = model.City,
                UserType = "Donor",
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return new AuthResult { Succeeded = false, Errors = ConvertErrors(result) };
            }

            await _userManager.AddToRoleAsync(user, "Donor");

            var donor = new Donor
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                IsEligible = true
            };

            await _donorRepo.AddDonorAsync(donor);

            await _signInManager.SignInAsync(user, isPersistent: false);

            return new AuthResult { Succeeded = true, UserType = "Donor" };
        }

        public async Task<AuthResult> RegisterHospital(RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.Phone,
                City = model.City,
                UserType = "Hospital",
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return new AuthResult { Succeeded = false, Errors = ConvertErrors(result) };
            }

            await _userManager.AddToRoleAsync(user, "Hospital");

            var hospital = new Hospital
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Name = model.FullName,
                Address = string.Empty,
                City = model.City,
                Phone = model.Phone,
                IsApproved = false
            };

            await _hospitalRepo.AddHospitalAsync(hospital);

            await _signInManager.SignInAsync(user, isPersistent: false);

            return new AuthResult { Succeeded = true, UserType = "Hospital" };
        }

        public async Task<AuthResult> RegisterBloodBank(RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.Phone,
                City = model.City,
                UserType = "BloodBank",
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return new AuthResult { Succeeded = false, Errors = ConvertErrors(result) };
            }

            await _userManager.AddToRoleAsync(user, "BloodBank");

            var bloodBank = new BloodBank
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Name = model.FullName,
                Address = string.Empty,
                City = model.City,
                Phone = model.Phone,
                IsApproved = false
            };

            await _bloodBankRepo.AddBloodBankAsync(bloodBank);

            await _signInManager.SignInAsync(user, isPersistent: false);

            return new AuthResult { Succeeded = true, UserType = "BloodBank" };
        }

        public async Task<AuthResult> Login(LoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = new[] { "Invalid email or password" }
                };
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            return new AuthResult
            {
                Succeeded = true,
                UserType = user?.UserType
            };
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        private static string[] ConvertErrors(IdentityResult result)
        {
            var errors = new List<string>();
            foreach (var e in result.Errors) errors.Add(e.Description);
            return errors.ToArray();
        }
    }
}