using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.Services.Implementation
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepo _appointmentRepo;

        public AppointmentService(IAppointmentRepo appointmentRepo)
        {
            _appointmentRepo = appointmentRepo;
        }

        public async Task<IEnumerable<AppointmentVM>> GetAppointmentsByDonorAsync(Guid donorId)
        {
            var appointments = await _appointmentRepo.GetByDonorIdAsync(donorId);
            return appointments.Select(a => new AppointmentVM
            {
                Id = a.Id,
                DonorId = a.DonorId,
                BloodBankId = a.BloodBankId,
                BloodBankName = a.BloodBank?.Name ?? string.Empty,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status ?? "Pending"
            });
        }

        public async Task<IEnumerable<AppointmentVM>> GetAppointmentsByBankAsync(Guid bloodBankId)
        {
            var appointments = await _appointmentRepo.GetByBankIdAsync(bloodBankId);
            return appointments.Select(a => new AppointmentVM
            {
                Id = a.Id,
                DonorId = a.DonorId,
                DonorName = a.Donor?.User?.UserName ?? string.Empty, 
                BloodBankId = a.BloodBankId,
                BloodBankName = a.BloodBank?.Name ?? string.Empty,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status ?? "Pending"
            });
        }

        public async Task<bool> CreateAppointmentAsync(CreateAppointmentVM model)
        {
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                DonorId = model.DonorId,
                BloodBankId = model.BloodBankId,
                AppointmentDate = model.AppointmentDate,
                Status = "Pending"
            };

            await _appointmentRepo.AddAsync(appointment);
            return await _appointmentRepo.SaveChangesAsync();
        }

        public async Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, string status)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
            if (appointment == null) return false;

            appointment.Status = status;
            _appointmentRepo.Update(appointment);
            return await _appointmentRepo.SaveChangesAsync();
        }
    }
}
