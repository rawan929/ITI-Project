using ITI.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Repo.Interface
{
    public interface IAppointmentRepo
    {
        Task<IEnumerable<Appointment>> GetByDonorIdAsync(Guid donorId);
        Task<IEnumerable<Appointment>> GetByBankIdAsync(Guid bloodBankId , DateTime? date=null);
        Task<bool> SlotTakenAsync(Guid bloodBankId, DateTime appointmentDate, string excludedStatus);
        Task<Donor?> GetDonorByUserIdAsync(Guid userId);
        Task<bool> DonorHasActiveAppointmentAsync(Guid donorId, DateTime from, string[] activeStatuses);
        Task<Appointment?> GetByIdAsync(Guid id);
        Task AddAsync(Appointment appointment);
        void Update(Appointment appointment);
        Task<bool> SaveChangesAsync();
    }
}
