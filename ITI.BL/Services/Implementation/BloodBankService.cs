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
    public class BloodBankService : IBloodBankService
    {
        private readonly IBloodBankRepo _bloodBankRepo;

        /// <summary>
        /// Minimum gap between whole-blood donations, in days. This matches the rule
        /// DonorService already uses for the donor dashboard, so both screens agree.
        /// Confirm the real interval with your blood bank's medical policy before launch.
        /// </summary>
        private const int DonationIntervalDays = 90;

        private static readonly string[] AllowedAppointmentStatuses =
            { "Scheduled", "Completed", "Cancelled", "NoShow" };

        public BloodBankService(IBloodBankRepo bloodBankRepo)
        {
            _bloodBankRepo = bloodBankRepo;
        }

        // =====================================================================
        // Dashboard
        // =====================================================================

        public async Task<BloodBankDashboardVM?> GetDashboardAsync(Guid userId)
        {
            var bank = await _bloodBankRepo.GetByUserIdAsync(userId);
            if (bank == null) return null;

            var inventory = await _bloodBankRepo.GetInventoryAsync(bank.Id);
            var donations = await _bloodBankRepo.GetDonationsAsync(bank.Id);
            var appointments = await _bloodBankRepo.GetAppointmentsAsync(bank.Id);

            var items = BuildInventoryItems(inventory, await _bloodBankRepo.GetAllBloodTypesAsync());

            var now = DateTime.UtcNow;

            return new BloodBankDashboardVM
            {
                BloodBankId = bank.Id,
                Name = bank.Name,
                City = bank.City,
                IsApproved = bank.IsApproved,

                TotalUnitsInStock = items.Sum(i => i.UnitsAvailable),
                TotalDonations = donations.Count,
                DonationsThisMonth = donations.Count(d =>
                    d.DonationDate.Year == now.Year && d.DonationDate.Month == now.Month),
                UpcomingAppointments = appointments.Count(a =>
                    a.Status == "Scheduled" && a.AppointmentDate >= now.Date),

                LowStockTypesCount = items.Count(i => i.IsLowStock),
                Inventory = items,
                RecentDonations = donations.Take(5).Select(MapDonation).ToList()
            };
        }

        // =====================================================================
        // Inventory
        // =====================================================================

        public async Task<BloodBankInventoryPageVM?> GetInventoryPageAsync(Guid userId)
        {
            var bank = await _bloodBankRepo.GetByUserIdAsync(userId);
            if (bank == null) return null;

            var bloodTypes = await _bloodBankRepo.GetAllBloodTypesAsync();
            var inventory = await _bloodBankRepo.GetInventoryAsync(bank.Id);

            return new BloodBankInventoryPageVM
            {
                BloodBankName = bank.Name,
                Items = BuildInventoryItems(inventory, bloodTypes),
                Adjust = new BloodBankAdjustInventoryVM
                {
                    AvailableBloodTypes = bloodTypes
                        .Select(b => new BloodTypeOptionVM { Id = b.Id, Name = b.Name })
                        .ToList()
                }
            };
        }

        public async Task<AdjustInventoryResult> AdjustInventoryAsync(
            Guid userId,
            BloodBankAdjustInventoryVM model)
        {
            var bank = await _bloodBankRepo.GetByUserIdAsync(userId);
            if (bank == null) return AdjustInventoryResult.BloodBankNotFound;
            if (!bank.IsApproved) return AdjustInventoryResult.BloodBankNotApproved;

            var bloodTypes = await _bloodBankRepo.GetAllBloodTypesAsync();
            if (bloodTypes.All(b => b.Id != model.BloodTypeId))
            {
                return AdjustInventoryResult.InvalidBloodType;
            }

            var item = await _bloodBankRepo.GetInventoryItemAsync(bank.Id, model.BloodTypeId);

            if (string.Equals(model.Operation, "Remove", StringComparison.OrdinalIgnoreCase))
            {
                if (item == null || item.UnitsAvailable < model.Units)
                {
                    return AdjustInventoryResult.InsufficientStock;
                }

                item.UnitsAvailable -= model.Units;
            }
            else
            {
                if (item == null)
                {
                    item = new BloodInventory
                    {
                        Id = Guid.NewGuid(),
                        BloodBankId = bank.Id,
                        BloodTypeId = model.BloodTypeId,
                        UnitsAvailable = model.Units
                    };
                    await _bloodBankRepo.AddInventoryAsync(item);
                }
                else
                {
                    item.UnitsAvailable += model.Units;
                }
            }

            await _bloodBankRepo.SaveChangesAsync();
            return AdjustInventoryResult.Success;
        }

        // =====================================================================
        // Donations
        // =====================================================================

        public async Task<BloodBankRecordDonationVM?> GetRecordDonationFormAsync(Guid userId, bool sameCityOnly = false)
        {
            var bank = await _bloodBankRepo.GetByUserIdAsync(userId);
            if (bank == null) return null;

            var donors = await _bloodBankRepo.GetDonorsByCityAsync(sameCityOnly ? bank.City : null);

            return new BloodBankRecordDonationVM
            {
                DonationDate = DateTime.UtcNow.Date,
                SameCityOnly = sameCityOnly,
                BankCity = bank.City,
                AvailableDonors = donors.Select(MapDonorOption).ToList()
            };
        }

        public async Task<RecordDonationResult> RecordDonationAsync(
            Guid userId,
            BloodBankRecordDonationVM model)
        {
            var bank = await _bloodBankRepo.GetByUserIdAsync(userId);
            if (bank == null) return RecordDonationResult.BloodBankNotFound;
            if (!bank.IsApproved) return RecordDonationResult.BloodBankNotApproved;

            var donationDate = model.DonationDate.Date;
            if (donationDate > DateTime.UtcNow.Date)
            {
                return RecordDonationResult.InvalidDate;
            }

            var donor = await _bloodBankRepo.GetDonorByIdAsync(model.DonorId);
            if (donor == null) return RecordDonationResult.DonorNotFound;

            // A donation has to be filed against a known blood type, otherwise the
            // units would land in the wrong bucket of the inventory.
            if (donor.BloodTypeId == null)
            {
                return RecordDonationResult.DonorBloodTypeMissing;
            }

            if (!model.OverrideEligibility && !IsDonorEligible(donor.LastDonationDate, donationDate))
            {
                return RecordDonationResult.DonorNotEligible;
            }

            // 1. Record the donation itself.
            var donation = new Donation
            {
                Id = Guid.NewGuid(),
                DonorId = donor.Id,
                BloodBankId = bank.Id,
                Units = model.Units,
                DonationDate = donationDate,
                Status = "Completed"
            };
            await _bloodBankRepo.AddDonationAsync(donation);

            // 2. Move the units into this bank's inventory.
            var item = await _bloodBankRepo.GetInventoryItemAsync(bank.Id, donor.BloodTypeId.Value);
            if (item == null)
            {
                item = new BloodInventory
                {
                    Id = Guid.NewGuid(),
                    BloodBankId = bank.Id,
                    BloodTypeId = donor.BloodTypeId.Value,
                    UnitsAvailable = model.Units
                };
                await _bloodBankRepo.AddInventoryAsync(item);
            }
            else
            {
                item.UnitsAvailable += model.Units;
            }

            // 3. Reset the donor's eligibility clock — but never move it backwards
            //    if an older donation is being back-filled.
            if (donor.LastDonationDate == null || donationDate > donor.LastDonationDate.Value.Date)
            {
                donor.LastDonationDate = donationDate;
            }
            donor.IsEligible = IsDonorEligible(donor.LastDonationDate, DateTime.UtcNow.Date);

            await _bloodBankRepo.SaveChangesAsync();
            return RecordDonationResult.Success;
        }

        public async Task<List<BloodBankDonationVM>> GetDonationHistoryAsync(Guid userId)
        {
            var bank = await _bloodBankRepo.GetByUserIdAsync(userId);
            if (bank == null) return new List<BloodBankDonationVM>();

            var donations = await _bloodBankRepo.GetDonationsAsync(bank.Id);
            return donations.Select(MapDonation).ToList();
        }

        // =====================================================================
        // Appointments
        // =====================================================================

        public async Task<List<BloodBankAppointmentVM>> GetAppointmentsAsync(Guid userId)
        {
            var bank = await _bloodBankRepo.GetByUserIdAsync(userId);
            if (bank == null) return new List<BloodBankAppointmentVM>();

            var appointments = await _bloodBankRepo.GetAppointmentsAsync(bank.Id);
            var today = DateTime.UtcNow.Date;

            return appointments.Select(a => new BloodBankAppointmentVM
            {
                AppointmentId = a.Id,
                DonorId = a.DonorId,
                DonorName = a.Donor?.User?.FullName ?? "Unknown donor",
                BloodTypeName = a.Donor?.BloodType?.Name ?? "Not set",
                Phone = a.Donor?.User?.PhoneNumber ?? string.Empty,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status,
                IsUpcoming = a.Status == "Scheduled" && a.AppointmentDate.Date >= today
            }).ToList();
        }

        public async Task<bool> UpdateAppointmentStatusAsync(Guid userId, Guid appointmentId, string status)
        {
            if (!AllowedAppointmentStatuses.Contains(status))
            {
                return false;
            }

            var bank = await _bloodBankRepo.GetByUserIdAsync(userId);
            if (bank == null) return false;

            var appointment = await _bloodBankRepo.GetAppointmentByIdAsync(appointmentId);

            // Never let one bank change another bank's appointment.
            if (appointment == null || appointment.BloodBankId != bank.Id)
            {
                return false;
            }

            appointment.Status = status;
            await _bloodBankRepo.SaveChangesAsync();
            return true;
        }

        // =====================================================================
        // Hospital requests
        // =====================================================================

        public async Task<List<BloodBankIncomingRequestVM>> GetIncomingRequestsAsync(
            Guid userId,
            bool sameCityOnly)
        {
            var bank = await _bloodBankRepo.GetByUserIdAsync(userId);
            if (bank == null) return new List<BloodBankIncomingRequestVM>();

            var requests = await _bloodBankRepo.GetPendingRequestsAsync(sameCityOnly ? bank.City : null);
            var inventory = await _bloodBankRepo.GetInventoryAsync(bank.Id);

            var stockByType = inventory.ToDictionary(i => i.BloodTypeId, i => i.UnitsAvailable);

            return requests
                .Select(r => new BloodBankIncomingRequestVM
                {
                    RequestId = r.Id,
                    HospitalName = r.Hospital?.Name ?? string.Empty,
                    HospitalCity = r.Hospital?.City ?? string.Empty,
                    HospitalPhone = r.Hospital?.Phone ?? string.Empty,
                    BloodTypeId = r.BloodTypeId,
                    BloodTypeName = r.BloodType?.Name ?? string.Empty,
                    UnitsRequired = r.UnitsRequired,
                    Urgency = r.Urgency,
                    Status = r.Status,
                    UnitsInStock = stockByType.TryGetValue(r.BloodTypeId, out var units) ? units : 0
                })
                // Emergencies first, then the ones this bank can actually cover.
                .OrderByDescending(r => r.Urgency == "Emergency")
                .ThenByDescending(r => r.CanFulfill)
                .ToList();
        }

        public async Task<FulfillRequestResult> FulfillRequestAsync(Guid userId, Guid requestId)
        {
            var bank = await _bloodBankRepo.GetByUserIdAsync(userId);
            if (bank == null) return FulfillRequestResult.BloodBankNotFound;
            if (!bank.IsApproved) return FulfillRequestResult.BloodBankNotApproved;

            var request = await _bloodBankRepo.GetBloodRequestByIdAsync(requestId);
            if (request == null) return FulfillRequestResult.RequestNotFound;
            if (request.Status != "Pending") return FulfillRequestResult.RequestAlreadyClosed;

            // Exact-type issue only. Substituting a compatible type is a medical
            // decision, so it is deliberately not done automatically here.
            var item = await _bloodBankRepo.GetInventoryItemAsync(bank.Id, request.BloodTypeId);
            if (item == null || item.UnitsAvailable < request.UnitsRequired)
            {
                return FulfillRequestResult.InsufficientStock;
            }

            item.UnitsAvailable -= request.UnitsRequired;
            request.Status = "Fulfilled";

            await _bloodBankRepo.SaveChangesAsync();
            return FulfillRequestResult.Success;
        }

        // =====================================================================
        // Profile
        // =====================================================================

        public async Task<BloodBankProfileVM?> GetProfileAsync(Guid userId)
        {
            var bank = await _bloodBankRepo.GetByUserIdAsync(userId);
            if (bank == null) return null;

            return new BloodBankProfileVM
            {
                Name = bank.Name,
                Address = bank.Address,
                City = bank.City,
                Phone = bank.Phone,
                Email = bank.User?.Email ?? string.Empty,
                IsApproved = bank.IsApproved
            };
        }

        public async Task<bool> UpdateProfileAsync(Guid userId, BloodBankProfileVM model)
        {
            var bank = await _bloodBankRepo.GetByUserIdAsync(userId);
            if (bank == null) return false;

            bank.Name = model.Name;
            bank.Address = model.Address;
            bank.City = model.City;
            bank.Phone = model.Phone;

            await _bloodBankRepo.UpdateBloodBankAsync(bank);
            return true;
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        private static bool IsDonorEligible(DateTime? lastDonationDate, DateTime asOf)
        {
            if (lastDonationDate == null) return true;
            return (asOf.Date - lastDonationDate.Value.Date).TotalDays >= DonationIntervalDays;
        }

        /// <summary>
        /// Returns a row for every blood type, including the ones with no stock,
        /// so the inventory grid never has holes in it.
        /// </summary>
        private static List<BloodBankInventoryItemVM> BuildInventoryItems(
            List<BloodInventory> inventory,
            List<BloodType> allBloodTypes)
        {
            var stockByType = inventory.ToDictionary(i => i.BloodTypeId, i => i.UnitsAvailable);

            return allBloodTypes
                .OrderBy(b => b.Id)
                .Select(b => new BloodBankInventoryItemVM
                {
                    BloodTypeId = b.Id,
                    BloodTypeName = b.Name,
                    UnitsAvailable = stockByType.TryGetValue(b.Id, out var units) ? units : 0
                })
                .ToList();
        }

        private static BloodBankDonationVM MapDonation(Donation d) => new()
        {
            DonationId = d.Id,
            DonorName = d.Donor?.User?.FullName ?? "Unknown donor",
            BloodTypeName = d.Donor?.BloodType?.Name ?? "Not set",
            Units = d.Units,
            DonationDate = d.DonationDate,
            Status = d.Status
        };

        private static BloodBankDonorOptionVM MapDonorOption(Donor d)
        {
            var nextEligible = d.LastDonationDate?.AddDays(DonationIntervalDays);

            return new BloodBankDonorOptionVM
            {
                DonorId = d.Id,
                FullName = d.User?.FullName ?? "Unknown donor",
                BloodTypeName = d.BloodType?.Name ?? "Not set",
                City = d.User?.City ?? string.Empty,
                LastDonationDate = d.LastDonationDate,
                NextEligibleDate = nextEligible,
                IsEligible = IsDonorEligible(d.LastDonationDate, DateTime.UtcNow.Date)
            };
        }
    }
}
