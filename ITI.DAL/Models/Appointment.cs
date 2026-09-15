using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Models
{
    public class Appointment
    {
        public Guid Id { get; set; }
        public Guid DonorId { get; set; }
        public Guid BloodBankId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = "Scheduled";

        // Navigation Properties
        public Donor Donor { get; set; } = null!;
        public BloodBank BloodBank { get; set; } = null!;
    }
}
