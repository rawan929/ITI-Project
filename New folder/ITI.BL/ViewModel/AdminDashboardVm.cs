using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.ViewModel
{
    public class AdminDashboardVm
    {
        public int TotalDonors { get; set; }
        public int ApprovedHospitals { get; set; }
        public int PendingHospitals { get; set; }
        public int ApprovedBloodBanks { get; set; }
        public int ActiveBloodRequests { get; set; }
    }
}
