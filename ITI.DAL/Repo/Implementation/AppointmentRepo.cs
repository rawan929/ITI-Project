using ITI.DAL.Context;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Repo.Implementation
{
    public class AppointmentRepo : IAppointmentRepo
    {
        private readonly AppDbcontext _context; 

        public AppointmentRepo(AppDbcontext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Appointment>> GetByDonorIdAsync(Guid donorId)
        {
            return await _context.Appointments
                .Include(a => a.BloodBank)
                .Where(a => a.DonorId == donorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByBankIdAsync(Guid bloodBankId, DateTime? date = null)
        {
            var query = _context.Appointments
                .AsNoTracking()
                .Include(a => a.BloodBank)
                .Include(a => a.Donor).ThenInclude(d => d.User)
                .Include(a => a.Donor).ThenInclude(d => d.BloodType)
                .Where(a => a.BloodBankId == bloodBankId);

            if (date.HasValue)
            {
                var start = date.Value.Date;
                var end = start.AddDays(1);
                query = query.Where(a => a.AppointmentDate >= start && a.AppointmentDate < end);
            }

            return await query.OrderBy(a => a.AppointmentDate).ToListAsync();
        }

        public async Task<Appointment?> GetByIdAsync(Guid id)
        {
            return await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> SlotTakenAsync(Guid bloodBankId, DateTime appointmentDate, string excludedStatus)
        {
            return await _context.Appointments.AnyAsync(a =>
                a.BloodBankId == bloodBankId &&
                a.AppointmentDate == appointmentDate &&
                a.Status != excludedStatus);
        }

        public async Task<Donor?> GetDonorByUserIdAsync(Guid userId)
        {
            return await _context.Donors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId);
        }

        public async Task<bool> DonorHasActiveAppointmentAsync(Guid donorId, DateTime from, string[] activeStatuses)
        {
            return await _context.Appointments.AnyAsync(a =>
                a.DonorId == donorId &&
                a.AppointmentDate >= from &&
                activeStatuses.Contains(a.Status));
        }

        public async Task AddAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
        }

        public void Update(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
