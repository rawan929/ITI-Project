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
    }
}
