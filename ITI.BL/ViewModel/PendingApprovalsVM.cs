using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.ViewModel
{
    public class PendingApprovalsVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public string Type { get; set; } = string.Empty; 
        public string Address { get; set; } = string.Empty;
    }
}
