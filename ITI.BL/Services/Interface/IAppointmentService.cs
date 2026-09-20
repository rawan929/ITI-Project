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
        Task<bool> CreateAppointmentAsync(CreateAppointmentVM model);
        Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, string status);
        Task<IEnumerable<AppointmentVM>> GetAppointmentsByBankAsync(Guid bloodBankId, DateTime? date = null);

        // جديد
        Task<List<AppointmentItem>> GetDonorAppointments(Guid donorId);   
        Task<(bool Success, string? Error)> BookAppointmentAsync(Guid userId, Guid bloodBankId, DateTime appointmentDate);
        Task<(bool Success, string? Error)> ChangeStatusAsync(Guid appointmentId, Guid bloodBankId, string newStatus);
    }
}