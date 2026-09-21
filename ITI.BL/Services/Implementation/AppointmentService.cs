using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.BLL.Services.Implementation
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IBloodBankRepo _bloodBankRepo;

        public AppointmentService(IBloodBankRepo bloodBankRepo)
        {
            _bloodBankRepo = bloodBankRepo;
        }

        public async Task<DonorAppointmentPageVM?> GetBookingPageAsync(Guid donorUserId, bool sameCityOnly)
        {
            var donor = await _bloodBankRepo.GetDonorByUserIdAsync(donorUserId);
            if (donor == null) return null;

            var city = donor.User?.City ?? string.Empty;
            var banks = await _bloodBankRepo.GetApprovedBloodBanksAsync(sameCityOnly ? city : null);
            var appointments = await _bloodBankRepo.GetAppointmentsByDonorAsync(donor.Id);

            var today = DateTime.UtcNow.Date;

            return new DonorAppointmentPageVM
            {
                DonorName = donor.User?.FullName ?? string.Empty,
                City = city,
                SameCityOnly = sameCityOnly,

                AvailableBloodBanks = banks.Select(b => new BloodBankOptionVM
                {
                    BloodBankId = b.Id,
                    Name = b.Name,
                    City = b.City,
                    Address = b.Address,
                    Phone = b.Phone
                }).ToList(),

                MyAppointments = appointments.Select(a => new DonorAppointmentRowVM
                {
                    AppointmentId = a.Id,
                    BloodBankName = a.BloodBank?.Name ?? string.Empty,
                    BloodBankCity = a.BloodBank?.City ?? string.Empty,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    CanCancel = a.Status == "Scheduled" && a.AppointmentDate.Date >= today
                }).ToList()
            };
        }

        public async Task<BookAppointmentResult> BookAsync(Guid donorUserId, BookAppointmentVM model)
        {
            var donor = await _bloodBankRepo.GetDonorByUserIdAsync(donorUserId);
            if (donor == null) return BookAppointmentResult.DonorNotFound;

            var bank = await _bloodBankRepo.GetByIdAsync(model.BloodBankId);
            if (bank == null || !bank.IsApproved) return BookAppointmentResult.BloodBankNotFound;

            if (model.AppointmentDate < DateTime.UtcNow)
            {
                return BookAppointmentResult.DateInThePast;
            }

            // One open booking per bank at a time keeps the bank's list readable.
            var existing = await _bloodBankRepo.GetAppointmentsByDonorAsync(donor.Id);
            if (existing.Any(a => a.BloodBankId == bank.Id && a.Status == "Scheduled"))
            {
                return BookAppointmentResult.AlreadyBooked;
            }

            await _bloodBankRepo.AddAppointmentAsync(new Appointment
            {
                Id = Guid.NewGuid(),
                DonorId = donor.Id,
                BloodBankId = bank.Id,
                AppointmentDate = model.AppointmentDate,
                Status = "Scheduled"
            });

            await _bloodBankRepo.SaveChangesAsync();
            return BookAppointmentResult.Success;
        }

        public async Task<bool> CancelAsync(Guid donorUserId, Guid appointmentId)
        {
            var donor = await _bloodBankRepo.GetDonorByUserIdAsync(donorUserId);
            if (donor == null) return false;

            var appointment = await _bloodBankRepo.GetAppointmentByIdAsync(appointmentId);

            // A donor can only cancel their own booking.
            if (appointment == null || appointment.DonorId != donor.Id) return false;
            if (appointment.Status != "Scheduled") return false;

            appointment.Status = "Cancelled";
            await _bloodBankRepo.SaveChangesAsync();
            return true;
        }
    }
}
