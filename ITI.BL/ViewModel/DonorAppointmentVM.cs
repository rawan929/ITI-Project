using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ITI.BLL.ViewModel
{
    public class DonorAppointmentPageVM
    {
        public string DonorName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool SameCityOnly { get; set; } = true;

        public List<BloodBankOptionVM> AvailableBloodBanks { get; set; } = new();
        public List<DonorAppointmentRowVM> MyAppointments { get; set; } = new();
        public BookAppointmentVM Booking { get; set; } = new();
    }

    public class BloodBankOptionVM
    {
        public Guid BloodBankId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class DonorAppointmentRowVM
    {
        public Guid AppointmentId { get; set; }
        public string BloodBankName { get; set; } = string.Empty;
        public string BloodBankCity { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool CanCancel { get; set; }
    }

    public class BookAppointmentVM
    {
        [Required(ErrorMessage = "Please choose a blood bank")]
        [Display(Name = "Blood Bank")]
        public Guid BloodBankId { get; set; }

        [Required(ErrorMessage = "Please choose a date and time")]
        [Display(Name = "Appointment Date")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; } = DateTime.UtcNow.Date.AddDays(1).AddHours(9);
    }

    public enum BookAppointmentResult
    {
        Success,
        DonorNotFound,
        BloodBankNotFound,
        DateInThePast,
        AlreadyBooked
    }
}
