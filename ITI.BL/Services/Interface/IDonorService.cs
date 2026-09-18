using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel.Donor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ITI.BLL.Services.Interface
{
    public interface IDonorService
    {
        Task<DonorDashboardViewModel?> GetDashboardData(Guid userId);
        Task<DonorProfileViewModel?> GetDonorProfile(Guid userId);        
        Task<AuthResult> UpdateDonorProfile(Guid userId, DonorProfileViewModel model); 
        Task<AuthResult> ChangePassword(Guid userId, ChangePasswordViewModel model);  
    }
}
