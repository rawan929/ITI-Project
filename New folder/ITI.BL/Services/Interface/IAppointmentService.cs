using ITI.BLL.ViewModel;
using System;
using System.Threading.Tasks;

namespace ITI.BLL.Services.Interface
{
    /// <summary>
    /// The donor side of the blood bank flow: choosing a bank and booking a slot.
    /// The bank sees the result on its Appointments page.
    /// </summary>
    public interface IAppointmentService
    {
        Task<DonorAppointmentPageVM?> GetBookingPageAsync(Guid donorUserId, bool sameCityOnly);
        Task<BookAppointmentResult> BookAsync(Guid donorUserId, BookAppointmentVM model);
        Task<bool> CancelAsync(Guid donorUserId, Guid appointmentId);
    }
}
