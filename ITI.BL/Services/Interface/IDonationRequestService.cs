using ITI.BLL.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.Services.Interface
{
    public interface IDonationRequestService
    {
        Task<bool> RespondToRequestAsync(Guid donorId, Guid requestId);

        Task<List<DonorResponseViewModel>> GetResponsesForRequestAsync(Guid requestId);
    }
}
