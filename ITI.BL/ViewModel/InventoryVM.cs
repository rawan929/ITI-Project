using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.ViewModel
{
    public class InvetoryItemVM
    {
        public int BloodTybeId { get; set; }
        public string BloodTybeName { get; set; } = string.Empty;
        public int UnitsAvailable { get; set; }
    }
    public class InventoryVM
    {
        public Guid BloodBankId { get; set; }
        public string BloodBankName { get; set; } = string.Empty;
        public List<InvetoryItemVM>invetoryItems { get; set; } = new List<InvetoryItemVM>();
    }
}
