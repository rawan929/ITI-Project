using ITI.BLL.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.Services.Interface
{
    public interface IDonationService
    {
        Task<bool> RecordDonationAsync(CreateDonationVM model);
        Task<IEnumerable<DonationVM>> GetDonationsByBankAsync(Guid bloodBankId);
        Task<IEnumerable<DonationVM>> GetDonationsByDonorAsync(Guid donorId);
        Task<List<DonationHistoryItem>> GetDonorDonationHistory(Guid donorId);  
        Task<(bool Success, string? Error)> MarkProcessedAsync(Guid donationId, Guid bloodBankId);
    }
}