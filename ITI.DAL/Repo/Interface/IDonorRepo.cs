using ITI.DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITI.DAL.Repo.Interface
{
    public interface IDonorRepo
    {
        Task<IEnumerable<Donor>> GetAllDonorsWithDetailsAsync();
        Task AddDonorAsync(Donor donor);
        Task<Donor?> GetByUserIdWithDetailsAsync(Guid userId);
        Task UpdateDonorAsync(Donor donor);                      
        Task<List<BloodType>> GetAllBloodTypesAsync();         
    }
}