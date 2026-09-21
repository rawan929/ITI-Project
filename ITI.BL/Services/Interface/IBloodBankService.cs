using ITI.BLL.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITI.BLL.Services.Interface
{
    /// <summary>
    /// All blood bank workflow. Every method takes the logged-in user's id and
    /// resolves the bank itself, so a bank can never act on another bank's data.
    /// </summary>
    public interface IBloodBankService
    {
        Task<BloodBankDashboardVM?> GetDashboardAsync(Guid userId);

        // Inventory
        Task<BloodBankInventoryPageVM?> GetInventoryPageAsync(Guid userId);
        Task<AdjustInventoryResult> AdjustInventoryAsync(Guid userId, BloodBankAdjustInventoryVM model);

        // Donations
        Task<BloodBankRecordDonationVM?> GetRecordDonationFormAsync(Guid userId);
        Task<RecordDonationResult> RecordDonationAsync(Guid userId, BloodBankRecordDonationVM model);
        Task<List<BloodBankDonationVM>> GetDonationHistoryAsync(Guid userId);

        // Appointments
        Task<List<BloodBankAppointmentVM>> GetAppointmentsAsync(Guid userId);
        Task<bool> UpdateAppointmentStatusAsync(Guid userId, Guid appointmentId, string status);

        // Hospital requests
        Task<List<BloodBankIncomingRequestVM>> GetIncomingRequestsAsync(Guid userId, bool sameCityOnly);
        Task<FulfillRequestResult> FulfillRequestAsync(Guid userId, Guid requestId);

        // Profile
        Task<BloodBankProfileVM?> GetProfileAsync(Guid userId);
        Task<bool> UpdateProfileAsync(Guid userId, BloodBankProfileVM model);
    }
}
