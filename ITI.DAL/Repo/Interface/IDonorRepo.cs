using ITI.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Repo.Interface
{
    public interface IDonorRepo
    {
        Task<IEnumerable<Donor>> GetAllDonorsWithDetailsAsync();
    }
}
