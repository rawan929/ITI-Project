using ITI.BLL.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.Services.Interface
{
    public interface IHospitalService
    {
        Task<HospitalProfileVM?> GetHospitalProfileAsync(Guid userId);
    }
}
