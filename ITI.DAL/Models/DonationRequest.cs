using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Models
{
    public class DonationRequest
    {
        public Guid Id { get; set; }
        public Guid BloodRequestId { get; set; }
        public Guid DonorId { get; set; }
        public string Status { get; set; } = "Pending";

        // Navigation Properties
        public BloodRequest BloodRequest { get; set; } = null!;
        public Donor Donor { get; set; } = null!;
    }
}
