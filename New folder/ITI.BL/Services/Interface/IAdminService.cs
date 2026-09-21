using ITI.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;
using ITI.BLL.ViewModel;
namespace ITI.BLL.Services.Interface
{
    public interface IAdminService
    {
        Task<AdminDashboardVm> GetDashboardStatsAsync();
        Task<List<DonorVM>> GetAllDonorsAsync();
        Task<List<PendingApprovalsVM>> GetPendingHospitalsAsync();
        Task<List<PendingApprovalsVM>> GetPendingBloodBanksAsync();
        /// <summary>Hospitals and blood banks waiting for approval, in one list.</summary>
        Task<List<PendingApprovalsVM>> GetPendingApprovalsAsync();
        Task<bool> ApproveHospitalAsync(Guid hospitalId);
        Task<bool> ApproveBloodBankAsync(Guid bloodBankId);
        Task<bool> ToggleUserStatusAsync(Guid userId);
        Task<List<ApplicationUser>> GetAllUsersAsync();
    }
}
