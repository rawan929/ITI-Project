using ITI.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Repo.Interface
{
  
    
        public interface IHospitalRepo
        {
            Task<List<Hospital>> GetPendingHospitalsAsync();
            Task<Hospital?> GetByIdAsync(Guid id);
            Task UpdateHospitalAsync(Hospital hospital);
            Task<int> GetApprovedCountAsync();
            Task<int> GetPendingCountAsync();
        }
    }

