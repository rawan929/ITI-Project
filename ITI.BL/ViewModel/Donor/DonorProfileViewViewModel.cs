using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.ViewModel.Donor
{
    public class DonorProfileViewViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string BloodTypeName { get; set; } = "Not set";
        public DateTime? DateOfBirth { get; set; }
        public DateTime? LastDonationDate { get; set; }
        public DateTime? EligibleFromDate { get; set; }
        public bool IsEligible { get; set; }

        public double? Weight { get; set; }
        public double? Height { get; set; }
        public string? KnownAllergies { get; set; }
        public string? ChronicConditions { get; set; }
    }
}
