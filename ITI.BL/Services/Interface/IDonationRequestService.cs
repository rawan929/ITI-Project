using ITI.BLL.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITI.BLL.Services.Interface
{
    public interface IDonationRequestService
    {
        /// <summary>A donor volunteering for a request they found themselves.</summary>
        Task<DonationResponseResult> RespondToRequestAsync(Guid donorId, Guid requestId);

        /// <summary>Everything in this donor's request inbox: invitations and past answers.</summary>
        Task<List<DonorInboxItemVM>> GetInboxAsync(Guid donorId);

        Task<RespondToInvitationResult> AcceptInvitationAsync(Guid donorId, Guid donationRequestId);

        Task<RespondToInvitationResult> DeclineInvitationAsync(Guid donorId, Guid donationRequestId);

        Task<List<DonorResponseViewModel>> GetResponsesForRequestAsync(Guid requestId);

        Task<List<DonorResponseHistoryVM>> GetDonorResponseHistoryAsync(Guid donorId);

        /// <summary>Approved blood banks a donor can choose from for a donation request, same-city ones first.</summary>
        Task<List<BloodBankOptionVM>> GetAvailableBloodBanksAsync(Guid donationRequestId);

        /// <summary>Books the appointment for an accepted invitation at the donor's chosen blood bank.</summary>
        Task<ScheduleAppointmentResult> ScheduleAppointmentAsync(Guid donorId, Guid donationRequestId, Guid bloodBankId, DateTime appointmentDate);
    }
}
