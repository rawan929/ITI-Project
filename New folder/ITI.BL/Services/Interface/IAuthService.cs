using System;
using System.Collections.Generic;
using System.Text;
using ITI.BLL.ViewModel.Account;
using System.Threading.Tasks;

namespace ITI.BLL.Services.Interface
{
    public class AuthResult
    {
        public bool Succeeded { get; set; }
        public string[] Errors { get; set; } = Array.Empty<string>();
        public string? UserType { get; set; }
    }

    public interface IAuthService
    {
        Task<AuthResult> RegisterDonor(RegisterViewModel model);
        Task<AuthResult> RegisterHospital(RegisterViewModel model);
        Task<AuthResult> RegisterBloodBank(RegisterViewModel model);
        Task<AuthResult> Login(LoginViewModel model);
        Task Logout();
    }
}
