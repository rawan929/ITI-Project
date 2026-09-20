using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.ViewModel
{
    public class DonationHistoryItem
    {
        public DateTime DonationDate { get; set; }
        public string BloodBankName { get; set; } = string.Empty;
        public int Units { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}