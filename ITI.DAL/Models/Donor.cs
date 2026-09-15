using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Models
{
    public class Donor
    {
        public Guid Id { get; set; } 
        public Guid UserId { get; set; }
        public int BloodTypeId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public DateTime? LastDonationDate { get; set; }
        public bool IsEligible { get; set; } = true;

        // Navigation Properties
        public ApplicationUser User { get; set; } = null!;
        public BloodType BloodType { get; set; } = null!;
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public ICollection<DonationRequest> DonationRequests { get; set; } = new List<DonationRequest>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
