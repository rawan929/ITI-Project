using ITI.BLL.ViewModel;
using System;
using System.Threading.Tasks;

namespace ITI.BLL.Services.Interface
{
    /// <summary>
    /// Answers one question: given a hospital blood request, which donors should we ask?
    ///
    /// It only matches. Telling the donors is NotificationService's job, so the two
    /// can change independently.
    /// </summary>
    public interface IDonorMatchingService
    {
        Task<MatchingResultVM?> FindMatchingDonorsAsync(Guid bloodRequestId);
    }
}
