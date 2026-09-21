using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Models
{
    public class BloodInventory
    {
        public Guid Id { get; set; }
        public Guid BloodBankId { get; set; }
        public int BloodTypeId { get; set; }
        public int UnitsAvailable { get; set; }

        // Navigation Properties
        public BloodBank BloodBank { get; set; } = null!;
        public BloodType BloodType { get; set; } = null!;
    }
}
