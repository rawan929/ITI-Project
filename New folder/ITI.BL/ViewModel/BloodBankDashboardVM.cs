using System;
using System.Collections.Generic;

namespace ITI.BLL.ViewModel
{
    /// <summary>
    /// Everything shown on the blood bank Overview page.
    /// </summary>
    public class BloodBankDashboardVM
    {
        public Guid BloodBankId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsApproved { get; set; }

        public int TotalUnitsInStock { get; set; }
        public int TotalDonations { get; set; }
        public int DonationsThisMonth { get; set; }
        public int UpcomingAppointments { get; set; }

        /// <summary>Blood types at or below the low-stock threshold.</summary>
        public int LowStockTypesCount { get; set; }

        public List<BloodBankInventoryItemVM> Inventory { get; set; } = new();
        public List<BloodBankDonationVM> RecentDonations { get; set; } = new();
    }

    public class BloodBankInventoryItemVM
    {
        public int BloodTypeId { get; set; }
        public string BloodTypeName { get; set; } = string.Empty;
        public int UnitsAvailable { get; set; }

        /// <summary>Units at or below this count are flagged as low stock in the UI.</summary>
        public const int LowStockThreshold = 5;

        public bool IsLowStock => UnitsAvailable <= LowStockThreshold;
        public bool IsOutOfStock => UnitsAvailable <= 0;
    }

    public class BloodBankDonationVM
    {
        public Guid DonationId { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public string BloodTypeName { get; set; } = string.Empty;
        public int Units { get; set; }
        public DateTime DonationDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
