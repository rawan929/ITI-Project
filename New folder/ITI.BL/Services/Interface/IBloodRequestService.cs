using ITI.BLL.ViewModel;
using ITI.DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITI.BLL.Services.Interface
{
    public interface IBloodRequestService
    {
        /// <summary>
        /// Creates the request and then runs matching + notification for it, so a
        /// hospital never has to hunt for donors by hand.
        /// </summary>
        Task<CreateRequestOutcome> CreateRequestAsync(CreateBloodRequestViewModel model, Guid hospitalId);

        /// <summary>Re-runs matching and invites any newly eligible donors.</summary>
        Task<int> MatchAndNotifyAsync(Guid bloodRequestId);

        /// <summary>The donors this request would reach, without inviting anyone.</summary>
        Task<MatchingResultVM?> PreviewMatchesAsync(Guid bloodRequestId);

        Task<IEnumerable<BloodRequest>> GetActiveRequestsAsync(
            string city,
            int? bloodTypeId);

        Task<bool> CloseRequestAsync(Guid requestId);
    }
}
