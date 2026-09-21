using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Models
{
    public class Donation
    {
        public Guid Id { get; set; }
        public Guid DonorId { get; set; }
        public Guid BloodBankId { get; set; }
        public int Units { get; set; }
        public DateTime DonationDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Completed";

        // Navigation Properties
        public Donor Donor { get; set; } = null!;
        public BloodBank BloodBank { get; set; } = null!;
    }
}
