using ITI.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Repo.Interface
{
    public interface IAppointmentRepo
    {
        Task<IEnumerable<Appointment>> GetByDonorIdAsync(Guid donorId);
        Task<IEnumerable<Appointment>> GetByBankIdAsync(Guid bloodBankId);
        Task<Appointment?> GetByIdAsync(Guid id);
        Task AddAsync(Appointment appointment);
        void Update(Appointment appointment);
        Task<bool> SaveChangesAsync();
    }
}
