using ITI.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.Services.Interface
{
    public interface IBloodBankService
    {
        Task<IEnumerable<BloodBank>> GetBloodBanksAsync();
    }
}
