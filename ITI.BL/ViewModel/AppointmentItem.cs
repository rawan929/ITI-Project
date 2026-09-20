using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.ViewModel
{
    public class AppointmentItem
    {
        public Guid Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string BloodBankName { get; set; } = string.Empty;
        public string BloodBankAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}