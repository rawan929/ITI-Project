using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ITI.BLL.ViewModel;

namespace ITI.BLL.Services.Interface
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentVM>> GetAppointmentsByDonorAsync(Guid donorId);
        Task<IEnumerable<AppointmentVM>> GetAppointmentsByBankAsync(Guid bloodBankId);
        Task<bool> CreateAppointmentAsync(CreateAppointmentVM model);
        Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, string status);
    }
}
