using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.ViewModel
{
    public class AppointmentVM
    {
        public Guid Id { get; set; }
        public Guid DonorId { get; set; }
        public string? DonorName { get; set; }
        public Guid BloodBankId { get; set; }
        public string? BloodBankName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? BloodType { get; set; }
        public string Status { get; set; } = "Pending";
    }
    public class CreateAppointmentVM
    {
        public string? DonorId { get; set; }
        public string? BloodBankId { get; set; }
        public DateTime AppointmentDate { get; set; }
    }
}
