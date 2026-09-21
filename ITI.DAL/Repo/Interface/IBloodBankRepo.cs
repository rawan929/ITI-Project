using ITI.DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITI.DAL.Repo.Interface
{
    public interface IBloodBankRepo
    {
        // ---- Blood bank ----
        Task<BloodBank?> GetByIdAsync(Guid id);
        Task<BloodBank?> GetByUserIdAsync(Guid userId);
        Task AddBloodBankAsync(BloodBank bloodBank);
        Task UpdateBloodBankAsync(BloodBank bloodBank);
        Task<int> GetApprovedCountAsync();
        Task<List<BloodBank>> GetPendingBloodBanksAsync();
        Task<List<BloodBank>> GetApprovedBloodBanksAsync(string? city);

        // ---- Inventory ----
        Task<List<BloodInventory>> GetInventoryAsync(Guid bloodBankId);
        Task<BloodInventory?> GetInventoryItemAsync(Guid bloodBankId, int bloodTypeId);
        Task AddInventoryAsync(BloodInventory inventory);

        // ---- Donations ----
        Task<List<Donation>> GetDonationsAsync(Guid bloodBankId);
        Task AddDonationAsync(Donation donation);

        // ---- Appointments ----
        Task<List<Appointment>> GetAppointmentsAsync(Guid bloodBankId);
        Task<Appointment?> GetAppointmentByIdAsync(Guid appointmentId);
        Task AddAppointmentAsync(Appointment appointment);

        // ---- Donors (needed when recording a walk-in donation) ----
        Task<Donor?> GetDonorByIdAsync(Guid donorId);
        Task<Donor?> GetDonorByUserIdAsync(Guid userId);
        Task<List<Appointment>> GetAppointmentsByDonorAsync(Guid donorId);
        Task<List<Donor>> GetDonorsByCityAsync(string city);

        // ---- Hospital blood requests the bank can fulfil ----
        Task<List<BloodRequest>> GetPendingRequestsAsync(string? city);
        Task<BloodRequest?> GetBloodRequestByIdAsync(Guid requestId);

        Task<List<BloodType>> GetAllBloodTypesAsync();

        /// <summary>
        /// Commits every change tracked so far. Used when a single operation has to
        /// touch more than one entity at once (e.g. fulfilling a request both
        /// deducts inventory and closes the request).
        /// </summary>
        Task SaveChangesAsync();
    }
}
