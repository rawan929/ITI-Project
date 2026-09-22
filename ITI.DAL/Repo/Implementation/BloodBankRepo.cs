using ITI.DAL.Context;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.DAL.Repo.Implementation
{
    public class BloodBankRepo : IBloodBankRepo
    {
        private readonly AppDbcontext _context;

        public BloodBankRepo(AppDbcontext context)
        {
            _context = context;
        }

        // ---------------- Blood bank ----------------

        public async Task<BloodBank?> GetByIdAsync(Guid id)
        {
            return await _context.BloodBanks.FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<BloodBank?> GetByUserIdAsync(Guid userId)
        {
            return await _context.BloodBanks
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.UserId == userId);
        }

        public async Task AddBloodBankAsync(BloodBank bloodBank)
        {
            await _context.BloodBanks.AddAsync(bloodBank);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBloodBankAsync(BloodBank bloodBank)
        {
            _context.BloodBanks.Update(bloodBank);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetApprovedCountAsync()
            => await _context.BloodBanks.CountAsync(b => b.IsApproved);

        public async Task<List<BloodBank>> GetPendingBloodBanksAsync()
        {
            return await _context.BloodBanks
                .Include(b => b.User)
                .Where(b => !b.IsApproved)
                .ToListAsync();
        }

        public async Task<List<BloodBank>> GetApprovedBloodBanksAsync(string? city)
        {
            var query = _context.BloodBanks.Where(b => b.IsApproved);

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(b => b.City == city);
            }

            return await query.OrderBy(b => b.Name).ToListAsync();
        }

        // ---------------- Inventory ----------------

        public async Task<List<BloodInventory>> GetInventoryAsync(Guid bloodBankId)
        {
            return await _context.BloodInventories
                .Include(i => i.BloodType)
                .Where(i => i.BloodBankId == bloodBankId)
                .OrderBy(i => i.BloodTypeId)
                .ToListAsync();
        }

        public async Task<BloodInventory?> GetInventoryItemAsync(Guid bloodBankId, int bloodTypeId)
        {
            return await _context.BloodInventories
                .Include(i => i.BloodType)
                .FirstOrDefaultAsync(i => i.BloodBankId == bloodBankId && i.BloodTypeId == bloodTypeId);
        }

        public async Task AddInventoryAsync(BloodInventory inventory)
        {
            await _context.BloodInventories.AddAsync(inventory);
        }

        // ---------------- Donations ----------------

        public async Task<List<Donation>> GetDonationsAsync(Guid bloodBankId)
        {
            return await _context.Donations
                .Include(d => d.Donor)
                    .ThenInclude(d => d.User)
                .Include(d => d.Donor)
                    .ThenInclude(d => d.BloodType)
                .Where(d => d.BloodBankId == bloodBankId)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();
        }

        public async Task AddDonationAsync(Donation donation)
        {
            await _context.Donations.AddAsync(donation);
        }

        // ---------------- Appointments ----------------

        public async Task<List<Appointment>> GetAppointmentsAsync(Guid bloodBankId)
        {
            return await _context.Appointments
                .Include(a => a.Donor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Donor)
                    .ThenInclude(d => d.BloodType)
                .Where(a => a.BloodBankId == bloodBankId)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(Guid appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Donor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }

        public async Task AddAppointmentAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
        }

        // ---------------- Donors ----------------

        public async Task<Donor?> GetDonorByIdAsync(Guid donorId)
        {
            return await _context.Donors
                .Include(d => d.User)
                .Include(d => d.BloodType)
                .FirstOrDefaultAsync(d => d.Id == donorId);
        }

        public async Task<Donor?> GetDonorByUserIdAsync(Guid userId)
        {
            return await _context.Donors
                .Include(d => d.User)
                .Include(d => d.BloodType)
                .FirstOrDefaultAsync(d => d.UserId == userId);
        }

        public async Task<List<Appointment>> GetAppointmentsByDonorAsync(Guid donorId)
        {
            return await _context.Appointments
                .Include(a => a.BloodBank)
                .Where(a => a.DonorId == donorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<List<Donor>> GetDonorsWithAppointmentAsync(Guid bloodBankId, string? city)
        {
            var donorIds = await _context.Appointments
                .Where(a => a.BloodBankId == bloodBankId)
                .Select(a => a.DonorId)
                .Distinct()
                .ToListAsync();

            var query = _context.Donors
                .Include(d => d.User)
                .Include(d => d.BloodType)
                .Where(d => d.BloodTypeId != null && d.User.IsActive && donorIds.Contains(d.Id));

            if (!string.IsNullOrWhiteSpace(city))
            {
                // Case/whitespace-insensitive match — "Cairo" vs "cairo" vs " Cairo "
                // are the same city and shouldn't quietly hide donors from the pool.
                var normalizedCity = city.Trim().ToLower();
                query = query.Where(d => d.User.City != null && d.User.City.Trim().ToLower() == normalizedCity);
            }

            return await query
                .OrderBy(d => d.User.FullName)
                .ToListAsync();
        }

        // ---------------- Hospital blood requests ----------------

        public async Task<List<BloodRequest>> GetPendingRequestsAsync(string? city)
        {
            var query = _context.BloodRequests
                .Include(r => r.Hospital)
                .Include(r => r.BloodType)
                .Where(r => r.Status == "Pending");

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(r => r.Hospital.City == city);
            }

            return await query.ToListAsync();
        }

        public async Task<BloodRequest?> GetBloodRequestByIdAsync(Guid requestId)
        {
            return await _context.BloodRequests
                .Include(r => r.Hospital)
                .Include(r => r.BloodType)
                .FirstOrDefaultAsync(r => r.Id == requestId);
        }

        public async Task<List<BloodType>> GetAllBloodTypesAsync()
            => await _context.BloodTypes.OrderBy(b => b.Id).ToListAsync();

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
