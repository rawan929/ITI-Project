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
        Task<DonorHeaderViewModel?> GetHeaderData(Guid userId, string activeTab);
        Task<DonorProfileViewModel?> GetDonorProfile(Guid userId);
        Task<DonorProfileViewViewModel?> GetDonorProfileView(Guid userId);
        Task<AuthResult> UpdateDonorProfile(Guid userId, DonorProfileViewModel model);
        Task<AuthResult> ChangePassword(Guid userId, ChangePasswordViewModel model);

        Task<List<NearbyRequestItemViewModel>> GetFindRequestsData(Guid userId, string? urgencyFilter);
        Task<AuthResult> RespondToRequest(Guid userId, Guid requestId);
        Task<string> GetDonorBloodTypeName(Guid userId);
    }
}
