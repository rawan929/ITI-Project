using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.ViewModel.Donor
{
    public class DonorDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string MemberSince { get; set; } = string.Empty; // "March 2023"
        public bool IsEligible { get; set; }

        public string BloodTypeName { get; set; } = "Not set";
        public int TotalDonations { get; set; }
        public DateTime? LastDonationDate { get; set; }
        public DateTime? NextEligibleDate { get; set; }
        public int? DaysUntilEligible { get; set; }
        public int ActiveRequestsCount { get; set; }
    }
}
