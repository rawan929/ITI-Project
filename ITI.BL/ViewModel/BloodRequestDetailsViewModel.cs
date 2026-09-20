using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.ViewModel
{
    public class BloodRequestDetailsViewModel
    {
        public Guid Id { get; set; }

        public string BloodType { get; set; } = string.Empty;

        public int UnitsRequired { get; set; }

        public string Urgency { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}
 