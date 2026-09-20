using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.Constants
{
    public static class AppointmentStatuses
    {
        public const string Scheduled = "Scheduled";   
        public const string Confirmed = "Confirmed";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
        public const string NoShow = "NoShow";

        public static string Display(string? status) => status switch
        {
            null or "" or Scheduled => "Pending",
            NoShow => "No Show",
            _ => status
        };
    }

    public static class DonationStatuses
    {
        public const string InTesting = "InTesting";
        public const string Completed = "Completed";   
        public const string Rejected = "Rejected";

        public static string Display(string? status) => status switch
        {
            InTesting => "In Testing",
            Completed => "Processed",
            _ => status ?? ""
        };
    }
}
