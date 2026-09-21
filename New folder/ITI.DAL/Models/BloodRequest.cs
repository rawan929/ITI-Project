using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Models
{
    public class BloodRequest
    {
        public Guid Id { get; set; }
        public Guid HospitalId { get; set; }
        public int BloodTypeId { get; set; }
        public int UnitsRequired { get; set; }
        public string Urgency { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";

        // Navigation Properties
        public Hospital Hospital { get; set; } = null!;
        public BloodType BloodType { get; set; } = null!;
        public ICollection<DonationRequest> DonationRequests { get; set; } = new List<DonationRequest>();
    }
}
