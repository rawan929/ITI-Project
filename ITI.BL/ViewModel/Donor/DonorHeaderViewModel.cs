using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.ViewModel.Donor
{
    public class DonorHeaderViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string MemberSince { get; set; } = string.Empty;
        public bool IsEligible { get; set; }
        public string ActiveTab { get; set; } = string.Empty; // "Dashboard" أو "Profile"
    }
}
