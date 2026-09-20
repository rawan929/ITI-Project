using ITI.BLL.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.Services.Interface
{
    public interface IDonationRequestService
    {
        Task<DonationResponseResult> RespondToRequestAsync(Guid donorId, Guid requestId);

        Task<List<DonorResponseViewModel>> GetResponsesForRequestAsync(Guid requestId);

        Task<List<DonorResponseHistoryVM>> GetDonorResponseHistoryAsync(Guid donorId);
    }
}