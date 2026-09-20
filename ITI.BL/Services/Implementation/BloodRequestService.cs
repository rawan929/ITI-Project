using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Context;
using ITI.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.Services.Implementation
{
    public class BloodRequestService : IBloodRequestService
    {
        private readonly AppDbcontext _context;

        public BloodRequestService(AppDbcontext context)
        {
            _context = context;
        }

        public async Task<CreateRequestResult> CreateRequestAsync(
            CreateBloodRequestViewModel model,
            Guid hospitalId)
        {
            var hospital = await _context.Hospitals
                .FirstOrDefaultAsync(h => h.Id == hospitalId);

            if (hospital == null)
            {
                return CreateRequestResult.HospitalNotFound;
            }

            if (!hospital.IsApproved)
            {
                return CreateRequestResult.HospitalNotApproved;
            }

            var request = new BloodRequest
            {
                Id = Guid.NewGuid(),
                HospitalId = hospitalId,
                BloodTypeId = model.BloodTypeId,
                UnitsRequired = model.UnitsRequired,
                Urgency = model.Urgency,
                Status = "Pending"
            };

            _context.BloodRequests.Add(request);

            await _context.SaveChangesAsync();

            return CreateRequestResult.Success;
        }

        public async Task<IEnumerable<BloodRequest>> GetActiveRequestsAsync(
            string city,
            int? bloodTypeId)
        {
            var query = _context.BloodRequests
                .Include(r => r.Hospital)
                .Include(r => r.BloodType)
                .Where(r => r.Status == "Pending");

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(r => r.Hospital.City == city);
            }

            if (bloodTypeId.HasValue)
            {
                query = query.Where(r => r.BloodTypeId == bloodTypeId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<bool> CloseRequestAsync(Guid requestId)
        {
            var request = await _context.BloodRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return false;

            request.Status = "Fulfilled";

            await _context.SaveChangesAsync();

            return true;
        }
    }
}