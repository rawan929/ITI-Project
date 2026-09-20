using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.ViewModel.Donor
{
    public class NearbyRequestItemViewModel
    {
        public Guid Id { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string BloodTypeName { get; set; } = string.Empty;
        public int UnitsRequired { get; set; }
        public string Urgency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
