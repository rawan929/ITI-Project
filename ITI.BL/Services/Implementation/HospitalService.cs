using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.Services.Implementation
{
    public class HospitalService : IHospitalService
    {
        private readonly AppDbcontext _context;

        public HospitalService(AppDbcontext context)
        {
            _context = context;
        }

        public async Task<HospitalProfileVM?> GetHospitalProfileAsync(Guid userId)
        {
            var hospital = await _context.Hospitals
                .FirstOrDefaultAsync(h => h.UserId == userId);

            if (hospital == null)
            {
                return null;
            }

            if (!hospital.IsApproved)
            {
                return null;
            }

            return new HospitalProfileVM
            {
                Id = hospital.Id,
                Name = hospital.Name,
                Address = hospital.Address,
                City = hospital.City,
                Phone = hospital.Phone,
                IsApproved = hospital.IsApproved
            };
        }
    }
}
