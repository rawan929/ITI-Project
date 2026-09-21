using ITI.BLL.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITI.BLL.Services.Interface
{
    /// <summary>
    /// Delivers a blood request to the donors the matching service picked.
    ///
    /// Right now "notifying" means creating the in-app invitation the donor sees in
    /// their requests inbox. Email or SMS would be added here, behind the same method,
    /// without the matching service needing to know.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>Invites the given donors and returns how many new invitations were created.</summary>
        Task<int> NotifyMatchingDonorsAsync(Guid bloodRequestId, List<MatchedDonorVM> donors);
    }
}
