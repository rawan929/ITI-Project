using System;
using System.Collections.Generic;

namespace ITI.BLL.ViewModel.Donor
{
    public class DonorDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string MemberSince { get; set; } = string.Empty;
        public bool IsEligible { get; set; }

        public string BloodTypeName { get; set; } = "Not set";
        public int TotalDonations { get; set; }
        public DateTime? LastDonationDate { get; set; }
        public DateTime? NextEligibleDate { get; set; }
        public int? DaysUntilEligible { get; set; }
    }
}
