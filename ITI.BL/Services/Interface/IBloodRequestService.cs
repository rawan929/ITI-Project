using ITI.BLL.ViewModel;
using ITI.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.Services.Interface
{
    public interface IBloodRequestService
    {
        Task CreateRequestAsync(CreateBloodRequestViewModel model, Guid hospitalId);

        Task<IEnumerable<BloodRequest>> GetActiveRequestsAsync(
            string city,
            int? bloodTypeId);

        Task<bool> CloseRequestAsync(Guid requestId);
    }
}
