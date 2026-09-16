using ITI.BLL.Services.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using ITI.DAL.Repo.Interface;
using ITI.DAL.Models;

namespace ITI.BLL.Services.Implementation
{
    public class BloodBankService:IBloodBankService
    {
        private readonly IBloodBankRepo _bloodBankRepo;
        public BloodBankService(IBloodBankRepo bloodBankRepo)
        {
            _bloodBankRepo = bloodBankRepo;
        }
        public async Task<IEnumerable<BloodBank>> GetBloodBanksAsync()
        {
            return (IEnumerable<BloodBank>)await _bloodBankRepo.GetAllApprovedCountAsync();
        }
    }
}
