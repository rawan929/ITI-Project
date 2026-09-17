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

        public async Task<IEnumerable<Appointment>> GetByBankIdAsync(Guid bloodBankId)
        {
            return await _context.Appointments
                .Include(a => a.Donor)
                .Where(a => a.BloodBankId == bloodBankId)
                .ToListAsync();
        }

        public async Task<Appointment?> GetByIdAsync(Guid id)
        {
            return await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id);
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
