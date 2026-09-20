using ITI.BLL.Constants;
using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;

namespace ITI.BLL.Services.Implementation
{
    public class AppointmentService : IAppointmentService
    {
        private const int MinDaysBetweenDonations = 56;

        private static readonly Dictionary<string, string[]> AllowedTransitions = new()
        {
            [AppointmentStatuses.Scheduled] = new[] { AppointmentStatuses.Confirmed, AppointmentStatuses.Cancelled },
            [AppointmentStatuses.Confirmed] = new[] { AppointmentStatuses.Completed, AppointmentStatuses.Cancelled, AppointmentStatuses.NoShow }
        };

        private readonly IAppointmentRepo _appointmentRepo;
        private readonly IBloodBankRepo _bloodBankRepo;
        private readonly IDonationRepo _donationRepo;

        public AppointmentService(IAppointmentRepo appointmentRepo, IBloodBankRepo bloodBankRepo, IDonationRepo donationRepo)
        {
            _appointmentRepo = appointmentRepo;
            _bloodBankRepo = bloodBankRepo;
            _donationRepo = donationRepo;
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

        public async Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, string status)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
            if (appointment == null) return false;

            appointment.Status = status;
            _appointmentRepo.Update(appointment);
            return await _appointmentRepo.SaveChangesAsync();
        }

        public async Task<bool> CreateAppointmentAsync(CreateAppointmentVM model)
        {
            if (!Guid.TryParse(model.DonorId, out var donorId) ||
                !Guid.TryParse(model.BloodBankId, out var bankId))
                return false;

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                DonorId = donorId,
                BloodBankId = bankId,
                AppointmentDate = model.AppointmentDate,
                Status = AppointmentStatuses.Scheduled
            };

            await _appointmentRepo.AddAsync(appointment);
            return await _appointmentRepo.SaveChangesAsync();
        }

        public async Task<IEnumerable<AppointmentVM>> GetAppointmentsByBankAsync(Guid bloodBankId, DateTime? date = null)
        {
            var appointments = await _appointmentRepo.GetByBankIdAsync(bloodBankId, date);
            return appointments.Select(a => new AppointmentVM
            {
                Id = a.Id,
                DonorId = a.DonorId,
                DonorName = a.Donor?.User?.FullName ?? string.Empty,
                BloodType = a.Donor?.BloodType?.Name,
                BloodBankId = a.BloodBankId,
                BloodBankName = a.BloodBank?.Name ?? string.Empty,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status
            });
        }

        public async Task<List<AppointmentItem>> GetDonorAppointments(Guid donorId)
        {
            var appointments = await _appointmentRepo.GetByDonorIdAsync(donorId);
            return appointments
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new AppointmentItem
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    BloodBankName = a.BloodBank?.Name ?? string.Empty,
                    BloodBankAddress = a.BloodBank?.Address ?? string.Empty,
                    Status = a.Status
                })
                .ToList();
        }

        public async Task<(bool Success, string? Error)> BookAppointmentAsync(
            Guid userId, Guid bloodBankId, DateTime appointmentDate)
        {
            var bank = await _bloodBankRepo.GetByIdAsync(bloodBankId);
            if (bank == null) return (false, "Blood bank not found.");
            if (!bank.IsApproved) return (false, "This blood bank is not approved yet.");

            if (appointmentDate <= DateTime.Now)
                return (false, "The appointment must be in the future.");

            var donor = await _appointmentRepo.GetDonorByUserIdAsync(userId);
            if (donor == null) return (false, "Donor profile not found.");
            if (!donor.IsEligible) return (false, "You are currently not eligible to donate.");

            if (donor.LastDonationDate.HasValue)
            {
                var nextAllowed = donor.LastDonationDate.Value.AddDays(MinDaysBetweenDonations);
                if (appointmentDate < nextAllowed)
                    return (false, $"You can donate again starting {nextAllowed:yyyy-MM-dd}.");
            }

            var activeStatuses = new[] { AppointmentStatuses.Scheduled, AppointmentStatuses.Confirmed };
            if (await _appointmentRepo.DonorHasActiveAppointmentAsync(donor.Id, DateTime.Now, activeStatuses))
                return (false, "You already have an upcoming appointment.");

            if (await _appointmentRepo.SlotTakenAsync(bloodBankId, appointmentDate, AppointmentStatuses.Cancelled))
                return (false, "This time slot is already booked.");

            await _appointmentRepo.AddAsync(new Appointment
            {
                Id = Guid.NewGuid(),
                DonorId = donor.Id,
                BloodBankId = bloodBankId,
                AppointmentDate = appointmentDate,
                Status = AppointmentStatuses.Scheduled
            });

            if (await _appointmentRepo.SaveChangesAsync()) return (true, null);
            return (false, "Could not save the appointment.");
        }

        public async Task<(bool Success, string? Error)> ChangeStatusAsync(
    Guid appointmentId, Guid bloodBankId, string newStatus)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
            if (appointment == null || appointment.BloodBankId != bloodBankId)
                return (false, "Appointment not found.");

            if (!AllowedTransitions.TryGetValue(appointment.Status, out var allowed) ||
                !allowed.Contains(newStatus))
                return (false, $"Cannot change the status from {appointment.Status} to {newStatus}.");

            appointment.Status = newStatus;
            {
                await _donationRepo.AddAsync(new Donation
                {
                    Id = Guid.NewGuid(),
                    DonorId = appointment.DonorId,
                    BloodBankId = appointment.BloodBankId,
                    Units = 1,
                    DonationDate = DateTime.UtcNow,
                    Status = DonationStatuses.InTesting
                });
            }

            if (await _appointmentRepo.SaveChangesAsync()) return (true, null);
            return (false, "Could not update the appointment.");
        }
    }
}