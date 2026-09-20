using System;
using System.Collections.Generic;
using System.Text;
using ITI.BLL.Constants;

namespace ITI.BLL.ViewModel
{
    public class InventoryItemVM
    {
        public int BloodTypeId { get; set; }
        public string BloodTypeName { get; set; } = string.Empty;
        public int UnitsAvailable { get; set; }
        public int Capacity { get; set; }
        public int Percent { get; set; }
        public string Level { get; set; } = string.Empty;  
    }

    public class InventoryVM
    {
        public Guid BloodBankId { get; set; }
        public string BloodBankName { get; set; } = string.Empty;
        public List<InventoryItemVM> Items { get; set; } = new();

        public int TotalUnits => Items.Sum(i => i.UnitsAvailable);
        public int CriticalTypes => Items.Count(i => i.Level == InventoryLevels.Critical);
        public int GoodTypes => Items.Count(i => i.Level == InventoryLevels.Good);
        public int TrackedTypes => Items.Count;
    }
}