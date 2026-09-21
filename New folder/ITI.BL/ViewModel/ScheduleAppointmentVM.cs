using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ITI.BLL.ViewModel
{
    /// <summary>The form a donor fills after accepting, to lock in a blood bank and time.</summary>
    public class ScheduleAppointmentVM
    {
        public Guid DonationRequestId { get; set; }

        public string HospitalName { get; set; } = string.Empty;
        public string BloodTypeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose a blood bank")]
        [Display(Name = "Blood Bank")]
        public Guid? BloodBankId { get; set; }

        /// <summary>Approved blood banks the donor can pick from (not posted back; refilled on redisplay).</summary>
        public List<BloodBankOptionVM> AvailableBloodBanks { get; set; } = new();

        [Required(ErrorMessage = "Please choose a date and time")]
        [Display(Name = "Appointment Date")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; } = DateTime.UtcNow.Date.AddDays(1).AddHours(9);
    }
}
